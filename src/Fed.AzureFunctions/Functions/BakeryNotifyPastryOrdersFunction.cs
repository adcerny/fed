using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class BakeryNotifyPastryOrdersFunction
    {

        private const string FuncName = "BakeryNotifyPastryOrdersFunction";
        private const string Schedule = "0 0 13 * * 1-5";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timerInfo,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, NotifyPastryOrders);

        public async static Task NotifyPastryOrders(ServicesBag bag)
        {
            var logger = bag.Logger;
            Guid PastryCategoryId = Guid.Parse(bag.Config.PastryCategoryId);

            // 1. Pull the forecast

            var deliveryDate = DateTime.Today.GetNextWorkingDay();
            var forecast = await bag.FedClient.GetSupplierForecastAsync((int)Suppliers.SevenSeeded, deliveryDate);

            // 2. Send notification

            var forecastRows = new List<(DateTime, string, string, int)>();

            //get pastry products only
            //filter to only include pastry orders
            var products = await bag.FedClient.GetProductsAsync(productCategoryId: PastryCategoryId);

            foreach (var date in forecast.Keys)
            {
                foreach (var pq in forecast[date])
                {
                    if (products.Any(p => p.SupplierSKU == pq.SupplierSKU))
                        forecastRows.Add((date, pq.SupplierSKU, pq.ProductName, pq.SupplierQuantity));
                }
            }

            var html = await EmailTemplates.ApplySevenSeededPastryNotificationTemplateAsync(forecastRows, deliveryDate);

            var title = $"Pastry orders from Fed";

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

        private static CloudTable GetTable(string tableName)
        {
            var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var account = CloudStorageAccount.Parse(conn);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(tableName);
            table.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            return table;
        }
    }
}