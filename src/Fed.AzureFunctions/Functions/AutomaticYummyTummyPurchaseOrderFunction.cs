using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class AutomaticYummyTummyPurchaseOrderFunction
    {
        private const string FuncName = "AutomaticYummyTummyPurchaseOrderFunction";
        private const string Schedule = "0 15 14 * * *";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timer,
            [Queue(QueueNames.SupplierPurchaseOrder)] IAsyncCollector<string> supplierPurchaseOrderQueue,
            ILogger logger)
            => FunctionRunner.RunWithArgAsync(logger, FuncName, supplierPurchaseOrderQueue, SendPurchaseOrder);

        public static async Task SendPurchaseOrder(IAsyncCollector<string> supplierPurchaseOrderQueue, ServicesBag bag)
        {
            var request = new SupplierPurchaseOrderRequest
            {
                SupplierId = (int)Suppliers.YummyTummy,
                EmailSubject = "Yummy Tummy Purchase Order",
                EmailAddresses = bag.Config.YummyTummyEmailAddresses.ToList()
            };

            var queueItem = JsonConvert.SerializeObject(request);

            await supplierPurchaseOrderQueue.AddAsync(queueItem);
        }
    }
}