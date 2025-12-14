using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class SupplierSendPurchaseOrderFunction
    {

        private const string FuncName = "SupplierSendPurchaseOrderFunction";

        [FunctionName(FuncName)]
        public static Task Run(
            [QueueTrigger(QueueNames.SupplierPurchaseOrder)]string queueItem,
            ILogger logger)
            => FunctionRunner.RunWithArgAsync(logger, FuncName, queueItem, SendPurchaseOrder);

        public async static Task SendPurchaseOrder(string queueItem, ServicesBag bag)
        {
            var logger = bag.Logger;

            var request = JsonConvert.DeserializeObject<SupplierPurchaseOrderRequest>(queueItem);

            // 1. Pull the forecast
            var deliveryDate = DateTime.Today.GetNextWorkingDay();
            var forecast = await bag.FedClient.GetSupplierForecastAsync(request.SupplierId, deliveryDate);

            // 2. Send notification

            var forecastRows = new List<(DateTime, string, string, int)>();
            var products = new List<Product>();

            var allProducts = await bag.FedClient.GetProductsAsync();
            products = allProducts.Where(p => p.SupplierId == request.SupplierId.ToString() && 
                                        (!request.ProductCategoryId.HasValue || p.ProductCategoryId == request.ProductCategoryId)).ToList();

            foreach (var date in forecast.Keys)
            {
                foreach (var pq in forecast[date])
                {
                    if (products.Any(p => p.SupplierSKU == pq.SupplierSKU))
                        forecastRows.Add((date, pq.SupplierSKU, pq.ProductName, pq.SupplierQuantity));
                }
            }

            string forecastUrl = $"https://supplier.fedteam.co.uk/{((Suppliers)request.SupplierId).ToString()}/forecast";
            var html = await EmailTemplates.ApplySupplierNotificationTemplateAsync(forecastRows, deliveryDate, forecastUrl);

            var email = new Email
            {
                FromAddress = "noreply@fedteam.co.uk",
                ToAddresses = request.EmailAddresses,
                CCs = bag.Config.FedBuyersEmailAddresses,
                BCCs = bag.Config.FedOpsEmailAddresses,
                Subject = request.EmailSubject,
                PlainText = "",
                HtmlText = html.ToString()
            };

            await bag.SendGridService.SendMessageAsync(email);
        }
    }
}