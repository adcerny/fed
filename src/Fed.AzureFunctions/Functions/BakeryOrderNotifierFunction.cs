using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class BakeryOrderNotifierFunction
    {
        public class BakeryForecastHashEntity : TableEntity
        {
            public BakeryForecastHashEntity() : base("bakery", "forecastHash")
            {
                Hash = "";
            }

            public BakeryForecastHashEntity(string hash) : base("bakery", "forecastHash")
            {
                Hash = hash;
            }

            public string Hash { get; set; }
        }

        private const string FuncName = "BakeryOrderNotifierFunction";
        private const string Schedule = "0 */15 * * * *";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timerInfo,
            [Table("SimpleKeyValueStore")] CloudTable keyValueStore,
            ILogger logger)
            => FunctionRunner.RunWithArgAsync(logger, FuncName, keyValueStore, NotifyBakery);

        private static string GenerateForecastKey(IDictionary<DateTime, IList<SupplierProductQuantity>> forecast)
        {
            // Create a key for the forecast
            var data = new StringBuilder();
            var dates = forecast.Keys;

            foreach (var date in dates)
            {
                data.Append(date.ToString("yyyyMMdd"));

                var productQuantities = forecast[date];

                foreach (var pq in productQuantities)
                {
                    data.Append($"{pq.SupplierSKU}{pq.SupplierQuantity}");
                }
            }

            return data.ToString();
        }

        public async static Task NotifyBakery(CloudTable keyValueStore, ServicesBag bag)
        {
            var logger = bag.Logger;

            // 1. Get the existing bakery forecast key:

            var readOps = TableOperation.Retrieve<BakeryForecastHashEntity>("bakery", "forecastHash");
            var tableResult = await keyValueStore.ExecuteAsync(readOps);

            var existingForecastKey = (tableResult.HttpStatusCode == 200 && tableResult.Result != null)
                ? (tableResult.Result as BakeryForecastHashEntity).Hash
                : "";

            logger.LogInformation($"HTTP Status Code from CloudTable Query: {tableResult.HttpStatusCode}");
            logger.LogInformation($"Existing Forecast Key: {existingForecastKey}");

            // 2. Pull the forecast

            var today = DateTime.Today;
            var toDate = today.AddWorkingDays(3);
            var forecast = await bag.FedClient.GetSupplierForecastAsync((int)Suppliers.SevenSeeded, toDate);

            // 3. Generate a new bakery forecast key:

            var latestForecastKey = GenerateForecastKey(forecast);

            logger.LogInformation($"Latest Forecast Key: {latestForecastKey}");

            // 4. Update key in table if it changed

            if (!latestForecastKey.Equals(existingForecastKey, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("The existing and latest forecast keys do not match anymore.");
                logger.LogInformation("Updating the latest forecast key in the Azure Table Storage...");

                var entity = new BakeryForecastHashEntity(latestForecastKey);
                var updateOps = TableOperation.InsertOrReplace(entity);
                await keyValueStore.ExecuteAsync(updateOps);
            }

            // 5. Strip old data from the key

            var strippedExistingForecastKey = string.Empty;

            if (!string.IsNullOrEmpty(existingForecastKey))
            {
                var idx = existingForecastKey.IndexOf(latestForecastKey.Substring(0, 8));
                strippedExistingForecastKey = existingForecastKey.Substring(idx >= 0 ? idx : 0);
            }

            logger.LogInformation($"Stripped Existing Forecast Key: {strippedExistingForecastKey}");

            // 6. Abort if orders haven't changed

            if (latestForecastKey.StartsWith(strippedExistingForecastKey, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogInformation("The existing forecast key matches the latest forecast key after being stripped. Skipping the automatic email notification.");
                return;
            }

            // 7. Send notification

            logger.LogInformation($"Keys were different, orders must have changed!");

            var forecastRows = new List<(DateTime, string, string, int)>();

            foreach (var date in forecast.Keys)
            {
                foreach (var pq in forecast[date])
                {
                    forecastRows.Add((date, pq.SupplierSKU, pq.ProductName, pq.SupplierQuantity));
                }
            }

            var html = await EmailTemplates.ApplySevenSeededNotificationTemplateAsync(forecastRows);

            var title = $"Order update from Fed";

            var email = new Email
            {
                FromAddress = "noreply@fedteam.co.uk",
                ToAddresses = bag.Config.SevenSeededEmailAddresses,
                CCs = bag.Config.FedBuyersEmailAddresses,
                BCCs = bag.Config.FedOpsEmailAddresses,
                Subject = title,
                PlainText = "",
                HtmlText = html.ToString()
            };

            await bag.SendGridService.SendMessageAsync(email);
        }
    }
}