using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class MinimumOrderService : IBakeryMinimumOrderService
    {
        private readonly IProductsHandler _productsHandler;

        public MinimumOrderService(IProductsHandler productsHandler)
        {
            _productsHandler = productsHandler ?? throw new ArgumentNullException(nameof(productsHandler));
        }

        public decimal MinimumOrderValue => 20m;

        private bool IsAllowableTopUpProduct(string sku)
        {
            // 27 Aug 2019 (Dustin):
            // Originally Chris & Lex decided that we should only use breads to top up our bakery order
            // to reach the minimum level, however today I've added SKU 10706 - Mini Viennoiserie Selection 12 Pieces
            // as an additional item on request by Lex.

            switch (sku)
            {
                // Seven Seeded Sourdough Loaf, Sliced - 800g 
                case "10676": return true;

                // Milk & Honey Sourdough Loaf, Sliced - 800g 
                case "10675": return true;

                // Hertfordshire Sourdough Loaf, Sliced - 800g 
                case "10677": return true;

                // Country Rye Loaf, Sliced - 800g 
                case "10678": return true;

                // Milk & Honey Sourdough Loaf, Unsliced - 400g
                case "100201": return true;

                // Country Rye Loaf, Unsliced - 400g
                case "10193": return true;

                // Seven Seeded Sourdough Loaf, Unsliced - 400g
                case "10194": return true;

                // Hertfordshire Sourdough Loaf, Unsliced - 400g
                case "10195": return true;

                // Classic Viennoiserie
                case "10681": return false;

                // Almond Viennoiserie
                case "10682": return false;

                // Butter Croissant
                case "10683": return false;

                // Pain au Chocolat
                case "10684": return false;

                // Pain aux Raisins
                case "10685": return false;

                // Almond Croissant
                case "10686": return false;

                // Almond Pain au Chocolat
                case "10687": return false;

                // Mini Butter Croissant - 10 pieces 
                case "10688": return false;

                // Mini Pain au Chocolat - 10 pieces 
                case "10689": return false;

                // Mini Pain au Raisin - 10 pieces 
                case "10690": return false;

                // Mini Viennoiserie Selection - 12 pieces
                case "10706": return false;

                // Unkown
                default: return false;
            }
        }

        // ToDo: Temp fix until we have the Supplier Price populated
        // and synced from Merchello. Need to ask Chris/Lex to do this!
        public decimal GetSupplierPrice(string sku)
        {
            switch (sku)
            {
                // Seven Seeded Sourdough Loaf, Sliced - 800g 
                case "10676": return 3.36m;

                // Milk & Honey Sourdough Loaf, Sliced - 800g 
                case "10675": return 3.36m;

                // Hertfordshire Sourdough Loaf, Sliced - 800g 
                case "10677": return 2.86m;

                // Country Rye Loaf, Sliced - 800g 
                case "10678": return 2.58m;

                // Milk & Honey Sourdough Loaf, Unsliced - 400g 
                case "100201": return 1.65m;

                // Country Rye Loaf, Unsliced - 400g
                case "10193": return 1.26m;

                // Seven Seeded Sourdough Loaf, Unsliced - 400g
                case "10194": return 1.65m;

                // Hertfordshire Sourdough Loaf, Unsliced - 400g
                case "10195": return 1.40m;

                // Classic Viennoiserie
                case "10681": return 3.42m;

                // Almond Viennoiserie
                case "10682": return 5.76m;

                // Butter Croissant
                case "10683": return 3.2m;

                // Pain au Chocolat
                case "10684": return 3.64m;

                // Pain aux Raisins
                case "10685": return 4.88m;

                // Almond Croissant
                case "10686": return 5.56m;

                // Almond Pain au Chocolat
                case "10687": return 5.96m;

                // Mini Butter Croissant - 10 pieces 
                case "10688": return 5.70m;

                // Mini Pain au Chocolat - 10 pieces 
                case "10689": return 6.80m;

                // Mini Pain au Raisin - 10 pieces 
                case "10690": return 6.80m;

                // Mini Viennoiserie Selection - 12 pieces
                case "10706": return 7.72m;

                // Unkown
                default: return 0;
            }
        }

        public async Task<IList<(Product, int)>> CalculateAdditionalRequiredBreadStockAsync(
            IList<SupplierProductQuantity> currentOrders)
        {
            var additionalProductQuantities = new List<(Product, int)>();
            var additionalQuantities = new Dictionary<string, int>();

            // Minimum stock is only required if there are any orders
            if (currentOrders == null || currentOrders.Count == 0)
                return additionalProductQuantities;

            var totalOrderValue = 0m;

            foreach (var item in currentOrders)
                totalOrderValue += GetSupplierPrice(item.SupplierSKU) * item.SupplierQuantity;

            // No additional orders required if minimum value is already met
            if (totalOrderValue >= MinimumOrderValue)
                return additionalProductQuantities;

            var products = await _productsHandler.ExecuteAsync(new GetProductsQuery(includeDeleted: false));

            // Get all allowable products sorted by lowest price first.
            var allowableTopUpProducts =
                products
                    .Where(p => p.IsSuppliedBySevenSeeded() && (IsAllowableTopUpProduct(p.SupplierSKU)))
                    .OrderBy(p => GetSupplierPrice(p.SupplierSKU))
                    .ToList();

            var i = 0;
            while (totalOrderValue < MinimumOrderValue)
            {
                if (i >= allowableTopUpProducts.Count)
                    i = 0;

                var topUpProduct = allowableTopUpProducts[i];
                var topUpPrice = GetSupplierPrice(topUpProduct.SupplierSKU);

                // If a product price is less or equal zero then skip and move on to the next product.
                // This could happen when a product has been added without any price info yet.
                if (topUpPrice > 0)
                {
                    if (additionalQuantities.ContainsKey(topUpProduct.SupplierSKU))
                        additionalQuantities[topUpProduct.SupplierSKU]++;
                    else
                        additionalQuantities.Add(topUpProduct.SupplierSKU, 1);

                    totalOrderValue += topUpPrice;
                }

                i++;
            }

            // Map skus/quantities to product objects with quantities:
            foreach (var additionalQuantity in additionalQuantities)
            {
                var product = allowableTopUpProducts.Single(p => p.SupplierSKU == additionalQuantity.Key);
                additionalProductQuantities.Add((product, additionalQuantity.Value));
            }

            return additionalProductQuantities;
        }

        public IList<(Product, int)> CalculateRequiredStockAsync(
            IList<int> minQuantities,
            IList<Product> products,
            IList<SupplierProductQuantity> currentOrders,
            RecurringOrder nextBufferOrder)
        {
            var additionalProductQuantities = new List<(Product, int)>();
            var additionalQuantities = new Dictionary<string, int>();

            // Minimum stock is only required if there are any orders
            if (currentOrders == null || currentOrders.Count == 0)
                return additionalProductQuantities;

            foreach (var product in products)
            {
                int supplierQuantity = currentOrders.FirstOrDefault(o => o.ProductCode == product.ProductCode)?.SupplierQuantity ?? 0;
                int bufferQuantity = nextBufferOrder?.OrderItems
                                                     .Where(i => i.ProductCode == product.ProductCode)?
                                                     .Sum(i => i.Quantity) ?? 0;

                int customerQuantity = (supplierQuantity - bufferQuantity);

                int minQuatity = minQuantities.Where(q => customerQuantity > 0 && 
                                                          customerQuantity <= q).FirstOrDefault();

                int needed = Math.Max(minQuatity - customerQuantity, 0);

                if (needed > 0)
                    additionalProductQuantities.Add((product, needed));
            }
            return additionalProductQuantities;
        }

        public async Task TopUpBreadOrderIfNeeded(IList<SupplierProductQuantity> currentOrders)
        {
            if (currentOrders is object && currentOrders.Count > 0)
            {
                var additionalQuantities = await CalculateAdditionalRequiredBreadStockAsync(currentOrders);

                foreach (var (product, quantity) in additionalQuantities)
                {
                    var productForOrder = currentOrders.SingleOrDefault(
                        p => Suppliers.SevenSeeded.MatchesSupplierId(p.SupplierId)
                        && p.SupplierSKU == product.SupplierSKU);

                    if (productForOrder == default(SupplierProductQuantity))
                    {
                        currentOrders.Add(
                            new SupplierProductQuantity
                            {
                                CustomerCount = 1,
                                Customers = "Fed auto order for minimum order",
                                FedQuantity = quantity,
                                SupplierQuantity = quantity,
                                ProductCode = product.ProductCode,
                                ProductName = product.ProductName,
                                SupplierId = product.SupplierId,
                                SupplierSKU = product.SupplierSKU
                            });
                    }
                    else
                    {
                        productForOrder.SupplierQuantity += quantity;
                        productForOrder.FedQuantity += quantity;
                    }
                }
            }
        }
    }
}