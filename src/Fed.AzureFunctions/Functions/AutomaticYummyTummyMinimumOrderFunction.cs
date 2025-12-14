using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class AutomaticYummyTummyMinimumOrderFunction
    {
        private const string FuncName = "AutomaticYummyTummyMinimumOrderFunction";
        private const string Schedule = "0 45 13 * * *";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timer,
            [Queue(QueueNames.SupplierMinimumOrder)] IAsyncCollector<string> supplierMinimumOrderQueue,
            ILogger logger)
            => FunctionRunner.RunWithArgAsync(logger, FuncName, supplierMinimumOrderQueue, TopUpOrder);

        public static async Task TopUpOrder(IAsyncCollector<string> supplierMinimumOrderQueue, ServicesBag bag)
        {
            var request = new SupplierTopUpRequest
            {
                SupplierId = (int)Suppliers.YummyTummy,
                MinQuantities = new List<int> { 5, 10 },
            };

            var queueItem = JsonConvert.SerializeObject(request);

            await supplierMinimumOrderQueue.AddAsync(queueItem);
        }
    }
}