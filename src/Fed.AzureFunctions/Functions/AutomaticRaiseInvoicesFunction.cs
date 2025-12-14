using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class AutomaticRaiseInvoicesFunction
    {
        private const string FuncName = "AutomaticRaiseInvoiesFunction";
        private const string Schedule = "0 0 13 * * FRI";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timer,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, RaiseInvoices);

        public static Task RaiseInvoices(ServicesBag bag)
        {
            var httpClient = new HttpClient();
            return httpClient.PostAsync(bag.Config.RaiseInvoicesUrl, null);
        }
    }
}