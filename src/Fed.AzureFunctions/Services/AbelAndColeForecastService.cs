using Fed.Api.External.AbelAndColeService;
using Fed.Api.External.MicrosoftTeams;
using Fed.AzureFunctions.Entities;
using Fed.Core.Data.Queries;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using Fed.Infrastructure.Data.SqlServer.Handlers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Services
{
    public static class AbelAndColeForecastService
    {
        public static async Task SendProductForecastAsync(
            string functionName,
            ServicesBag bag,
            DateTime startDate,
            int forecastDays,
            bool notifyOnSuccess)
        {
            var logger = bag.Logger;
            var fedClient = bag.FedClient;
            var acClient = bag.AbelAndColeClient;
            var fedBot = bag.FedBot;

            // 1. Retrieve all Abel & Cole products:
            // -------------------------------

            var productsHandler = new ProductsHandler(bag.SqlConfig);
            var products = await productsHandler.ExecuteAsync(new GetProductsQuery());
            var abelAndColeProducts = products.Where(p => p.IsSuppliedByAbelAndCole()).ToList();

            // 2. Init default zero quantities:
            // -------------------------------

            var defaultProductOrders = new Dictionary<string, int>();

            foreach (var p in abelAndColeProducts)
                defaultProductOrders.Add(p.SupplierSKU, 0);

            // 3. Get product forecast for all dates:
            // -------------------------------

            logger.LogInformation($"Loading forecast data for Abel & Cole...");
            var forecast =
                await bag.SuppliersService.GetSupplierForecastAsync((int)Suppliers.AbelAndCole, Date.Today.AddDays(forecastDays));

            // 4. Request product quantities from Supplier:
            // -------------------------------

            var warnings = new List<string>();
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // NCall the supplier API and create forecast items:
            foreach (var date in forecast.Keys)
            {
                var now = DateTime.Now.ToBritishTime();
                var abelAndColeCutOffTime = new DateTime(now.Year, now.Month, now.Day, 18, 30, 0);

                if (date.Value == DateTime.Today.GetNextWorkingDay() && now > abelAndColeCutOffTime)
                    continue;

                if (date.Value.DayOfWeek == DayOfWeek.Saturday || date.Value.DayOfWeek == DayOfWeek.Sunday)
                    continue;

                var productQuantitiesForDate = forecast[date];
                var invalidProducts = productQuantitiesForDate.Where(p => !int.TryParse(p.SupplierSKU, out _)).ToList();

                foreach (var p in invalidProducts)
                {
                    var warning = $"Couldn't place order on {date.Value.ToString("yyyy-MM-dd")} for product with invalid SKU: {p.SupplierSKU}.";
                    warnings.Add(warning);
                    logger.LogWarning(warning);
                    productQuantitiesForDate.Remove(p);
                    continue;
                }

                logger.LogInformation($"Getting Abel & Cole products on order for {date.Value.ToString("yyyy-MM-dd")}");
                var currentOrderItems = await acClient.TryGetOrderAsync(date) ?? new List<ProductQuantity>();
                
                List<ProductQuantity> orderUpdates = GetOrderUpdates(productQuantitiesForDate, currentOrderItems);

                if ((orderUpdates?.Count() ?? 0) == 0)
                    logger.LogInformation($"No order changes needed for {date.Value.ToString("yyyy-MM-dd")}");

                foreach (var order in orderUpdates)
                {
                    var productQuantity =
                        productQuantitiesForDate.SingleOrDefault(
                            x => Suppliers.AbelAndCole.MatchesSupplierId(x.SupplierId)
                            && x.SupplierSKU.Equals(order.ProductId.ToString()));

                    var quantityRequested = productQuantity == null ? 0 : productQuantity.SupplierQuantity;

                    var sku = order.ProductId;

                    var (quantityPlaced, errorMessage) = await acClient.TrySendOrderAsync(date, new ProductQuantity { ProductId = sku, Quantity = quantityRequested });

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        var errorDetails = $"Calling the A&C API for order on {date.Value.ToString("yyyy-MM-dd")}  for product {sku} x {quantityPlaced} failed.";
                        logger.LogWarning(errorDetails);
                        logger.LogWarning(errorMessage);
                        warnings.Add($"{errorDetails} Error message was: {errorMessage}");
                    }
                    if (quantityRequested != quantityPlaced)
                    {
                        var prd = products.SingleOrDefault(p => p.SupplierSKU.Equals(sku.ToString()));
                        var productCode = prd?.ProductCode ?? "(Code not specified)";

                        var warning = $"Product quantity could not be fulfilled for product {productCode} (SupplierSKU: {sku}) on {date.Value.ToString("yyyy-MM-dd")}. Quantity requested was {quantityRequested}. Quantity available was {quantityPlaced}.";
                        warnings.Add(warning);
                        logger.LogWarning(warning);
                    }
                    else
                    {
                        logger.LogInformation($"{date.Value.ToString("yyyy-MM-dd")} {sku} x {quantityRequested} successfully placed");
                    }
                }

                if ((orderUpdates?.Count() ?? 0) > 0)
                {
                    var updatedOrderItems = await acClient.TryGetOrderAsync(date) ?? new List<ProductQuantity>();
                    logger.LogInformation($"Logging {updatedOrderItems?.Count ?? 0} products on order at A&C for {date.Value.ToString("yyyy-MM-dd")} after update:");
                    logger.LogInformation(JsonConvert.SerializeObject(updatedOrderItems));
                }
            }

            stopWatch.Stop();

            var ts = stopWatch.Elapsed;
            var elapsedTime =
                string.Format(
                    "{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);

            logger.LogInformation("RunTime " + elapsedTime);

            // 5. Notify Teams with warnings:
            // -------------------------------

            if (warnings != null && warnings.Count > 0)
                await fedBot.SendMessage(TeamsCard.CreateWarning(warnings, functionName));

            // 6. Notify Teams on success:
            // -------------------------------

            if (notifyOnSuccess)
            {
                var subTitle =
                    forecast.Keys.Count == 1
                    ? "A total of 1 forecast has been placed."
                    : $"A total of {forecast.Keys.Count} forecasts have been placed.";

                var urlActions = new List<KeyValuePair<string, string>>();

                foreach (var date in forecast.Keys.Take(Math.Min(5, forecast.Keys.Count)))
                    urlActions.Add(
                        new KeyValuePair<string, string>(
                            $"View Orders for {date.Value.ToString("dd/MM/yyyy")}",
                            $"{bag.Config.FedPortalUrl}/supplier/8?deliveryDate={date.Value.ToString("yyyy-MM-dd")}"));

                await fedBot.SendMessage(
                    TeamsCard.Create(
                        CardType.SupplierOrder,
                        functionName,
                        $"Product orders have been placed for supplier Abel & Cole.",
                        subTitle,
                        string.Empty,
                        null,
                        null,
                        urlActions));
            }
        }

        private static List<ProductQuantity> GetOrderUpdates(IList<SupplierProductQuantity> productQuantitiesForDate, List<ProductQuantity> currentOrderItems)
        {
            var ordersToUpdate = productQuantitiesForDate.Where(o => !currentOrderItems.Any(c => c.ProductId.ToString() == o.SupplierSKU && c.Quantity == o.SupplierQuantity)).ToList();
            var ordersToDelete = currentOrderItems.Where(c => !productQuantitiesForDate.Any(o => o.SupplierSKU == c.ProductId.ToString())).ToList();

            var ordersToPlace = ordersToUpdate.Select(o => new ProductQuantity { ProductId = int.Parse(o.SupplierSKU), Quantity = o.SupplierQuantity }).ToList();
            ordersToPlace.AddRange(ordersToDelete.Select(o => new ProductQuantity { ProductId = o.ProductId, Quantity = 0 }).ToList());
            return ordersToPlace;
        }
    }
}
