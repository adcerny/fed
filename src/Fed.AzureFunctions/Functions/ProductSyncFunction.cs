using Fed.Api.External.MicrosoftTeams;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.ValueTypes;
using Fed.Infrastructure.Data.SqlServer.Handlers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class ProductSyncFunction
    {
        private const string FuncName = "ProductSyncFunction";

        [FunctionName(FuncName)]
        public static Task<HttpResponseMessage> Run(
            [HttpTrigger(Route = "productSync")] HttpRequestMessage req,
            ILogger logger)
            => FunctionRunner.RunWebAsync(logger, FuncName, req, ProductSync);

        public static async Task<HttpResponseMessage> ProductSync(HttpRequestMessage req, ServicesBag bag)
        {
            var logger = bag.Logger;
            var merchelloClient = bag.MerchelloClient;
            var productsHandler = new ProductsHandler(bag.SqlConfig);

            var fedProducts = await productsHandler.ExecuteAsync(new GetProductsQuery(includeDeleted: true));
            logger.LogInformation($"Retrieved {fedProducts.Count} products from the Fed Service.");

            var merchelloProducts = await merchelloClient.GetProductsAsync();

            // ---------------------
            // Retry the retrieval of products from Merchello 3 times before failing:

            var retries = 0;

            while (retries < 3 && merchelloProducts == null)
            {
                // Increase the delay with each re-try
                await Task.Delay(TimeSpan.FromSeconds(15 + 15 * retries));

                merchelloProducts = await merchelloClient.GetProductsAsync();

                retries++;
            }

            if (merchelloProducts == null)
                throw new Exception("The Merchello API has not returned any products. This is typically the case when the API has been down or had an internal error.");

            // ---------------------

            logger.LogInformation($"Retrieved {merchelloProducts.Count} products from the Merchello Service.");

            var remainingProductIds = new List<string>();
            var warnings = new List<string>();

            logger.LogInformation("Starting to synchronise the product catalogue...");

            var importedProducts = new List<Product>();

            foreach (var merchelloProduct in merchelloProducts)
            {
                // Don't sync products which don't have a valid SupplierId
                if (string.IsNullOrWhiteSpace(merchelloProduct.Supplier))
                {
                    logger.LogWarning("Skipping the sync of product '{product}', because it didn't have a valid Supplier ID.", merchelloProduct.Name);
                    warnings.Add($"Couldn't sync product '{merchelloProduct.Name}', because it didn't have a valid SupplierId.");
                    continue;
                }

                foreach (var variant in merchelloProduct.Variants)
                {
                    // Don't sync product variants which don't have a valid SupplierSKU
                    if (string.IsNullOrWhiteSpace(variant.SupplierSku))
                    {
                        logger.LogWarning("Skipping the sync of product '{product}', because it didn't have a SupplierSKU.", variant.Name);
                        warnings.Add($"Couldn't sync product variant '{variant.Name}', because it didn't have a SupplierSKU.");
                        continue;
                    }

                    // Don't sync A&C products which have an invalid SupplierSKU
                    else if (Suppliers.AbelAndCole.MatchesSupplierId(merchelloProduct.Supplier) && !int.TryParse(variant.SupplierSku, out int sku))
                    {
                        logger.LogWarning("Skipping the sync of product '{product}', because it didn't have a valid SupplierSKU. The supplier SKU was expected to be an integer, but was instead: {sku}", variant.Name, variant.SupplierSku);
                        warnings.Add($"Couldn't sync product variant '{variant.Name}', because it didn't have a valid SupplierSKU. The SKU was expected to be an integer but was '{variant.SupplierSku}'.");
                        continue;
                    }

                    else if (importedProducts.Where(p => p.SupplierSKU == variant.SupplierSku && p.SupplierId == p.SupplierId).Any())
                    {
                        logger.LogWarning("Skipping the sync of product '{product}', because there is already a product of supplier SKU {sku} for supplier {supplier}", variant.Name, variant.SupplierSku, merchelloProduct.Supplier);
                        warnings.Add($"Couldn't sync product variant '{variant.Name}', because there is already a product of supplier SKU {variant.SupplierSku} for supplier {merchelloProduct.Supplier}'.");
                        continue;
                    }

                    // Store the variant key to the remaining product list
                    remainingProductIds.Add(variant.Key.ToLower());

                    // Try to find the matching fed product
                    var fedProduct = fedProducts.SingleOrDefault(p => p.Id.Equals(variant.Key, StringComparison.OrdinalIgnoreCase));

                    var updatedFedProduct =
                        new Product
                        {
                            Id = variant.Key,
                            ProductGroup = GetPickingCategoryFromSku(variant.Sku),
                            ProductCode = variant.Sku,
                            ProductName = variant.Name,
                            SupplierId = merchelloProduct.Supplier,
                            SupplierSKU = variant.SupplierSku,
                            Price = variant.Price,
                            // TEMP FIX: When CogWorks release the new API which returns null then we can revert this to a simple assign statement
                            SalePrice = !variant.SalePrice.HasValue || variant.SalePrice.Value == 0 ? null : variant.SalePrice,
                            IsTaxable = merchelloProduct.Taxable,
                            IconCategory = merchelloProduct.CategoryIcon ?? "",
                            IsShippable = merchelloProduct.Shippable,
                            ProductCategoryId = merchelloProduct.ProductCategoryId
                        };

                    if (fedProduct == default(Product))
                    {
                        // Create a new fed product if the id didn't match any key
                        logger.LogInformation($"Inserting new product '{variant.Name}' into database...");
                        await productsHandler.ExecuteAsync(new CreateCommand<Product>(updatedFedProduct));
                    }
                    else
                    {
                        // Update if a fed product with the same id exist
                        logger.LogInformation($"Updating existing product '{variant.Name}' in database...");
                        await productsHandler.ExecuteAsync(new UpdateCommand<Product>(fedProduct.Id, updatedFedProduct));
                    }
                    importedProducts.Add(updatedFedProduct);
                }
            }



            logger.LogInformation("Finding products to be deleted");

            // Filter all products which have to get deleted:
            var fedProductsToDelete = fedProducts.Where(p => !remainingProductIds.Contains(p.Id.ToLower()) && !p.IsDeleted).ToList();

            logger.LogInformation($"{(fedProductsToDelete == null ? 0 : fedProductsToDelete.Count)} products to be deleted");

            if (fedProductsToDelete != null && fedProductsToDelete.Count > 0)
            {
                var recurringOrdersHandler = new RecurringOrdersHandler(bag.SqlConfig);
                var recurringOrdersQuery =
                    new GetRecurringOrdersQuery(
                        DateRange.TodayUntilEnd(),
                        includeExpired: false,
                        includeFromDeletedAccounts: false,
                        includeFromCancelledAccounts: true,
                        includeFromDemoAccounts: true);
                var recurringOrders = await recurringOrdersHandler.ExecuteAsync(recurringOrdersQuery);

                foreach (var fedProductToDelete in fedProductsToDelete)
                {
                    logger.LogInformation($"Removing old (or duplicate) product '{fedProductToDelete.ProductName}' from database...");

                    var isCurrentlyOrdered =
                        recurringOrders.Any(r => r.OrderItems.Any(ri => ri.ProductId.Equals(fedProductToDelete.Id, StringComparison.OrdinalIgnoreCase)));

                    await productsHandler.ExecuteAsync(new DeleteCommand<string>(fedProductToDelete.Id));

                    if (isCurrentlyOrdered)
                    {
                        warnings.Add($"The product with code {fedProductToDelete.ProductCode} and SKU {fedProductToDelete.SupplierSKU} has been removed from Merchello but is currently part of an existing recurring order.");
                    }
                }
            }

            logger.LogInformation($"Sending teams notification");

            // Send Teams notification with warnings
            if (warnings != null && warnings.Count > 0)
                await bag.FedBot.SendMessage(TeamsCard.CreateWarning(warnings, FuncName));

            return req.CreateResponse(HttpStatusCode.OK, $"The product sync finished successfully with {warnings.Count} warnings. Please check the Microsoft Teams or the logs for more information.");
        }

        private static string GetPickingCategoryFromSku(string productCode)
        {
            var abbr = productCode.Trim().Substring(0, 2);

            switch (abbr)
            {
                case "FV": return "Fruit & Veg";
                case "BK": return "Bakery";
                case "EG": return "Eggs";
                case "DY": return "Dairy";
                case "CD": return "Chilled Drinks";
                case "DL": return "Deli";
                case "AL": return "Alcohol";
                case "AD": return "Ambient Drinks";
                case "DA": return "Dairy Alts";
                case "PK": return "Packaged";
                case "PM": return "Prepared Meals";
                default: return "Unkown";
            }
        }
    }
}