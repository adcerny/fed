using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.AzureFunctions.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class SupplierForecastFunction
    {
        // CRON Expression:
        // https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-timer#cron-expressions

        private const string FuncName = "SupplierForecastFunction";
        private const string Schedule = "0 10 * * * *";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timerInfo,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, PlaceAbelAndColeForecast);

        public static Task PlaceAbelAndColeForecast(ServicesBag bag) =>
            AbelAndColeForecastService.SendProductForecastAsync(
                FuncName,
                bag,
                DateTime.Today.AddDays(1),
                14,
                false);
    }
}