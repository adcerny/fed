using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class AutomaticProductSyncFunction
    {
        private const string FuncName = "AutomaticProductSyncFunction";
        private const string Schedule = "0 45 * * * *";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timer,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, ProductSync);

        public static Task ProductSync(ServicesBag bag)
        {
            var httpClient = new HttpClient();
            return httpClient.GetAsync(bag.Config.ProductSyncUrl);
        }
    }
}