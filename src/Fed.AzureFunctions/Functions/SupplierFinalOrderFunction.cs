using Fed.Api.External.MicrosoftTeams;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.AzureFunctions.Services;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class SupplierFinalOrderFunction
    {
        private const string FuncName = "SupplierFinalOrderFunction";

        [FunctionName(FuncName)]
        public static Task Run(
            [QueueTrigger(QueueNames.SupplierFinalOrder)]string queueItem,
            ILogger logger)
            => FunctionRunner.RunWithArgAsync(logger, FuncName, queueItem, PlaceFinalOrderWithSuppliers);

        public static async Task PlaceFinalOrderWithSuppliers(string queueItem, ServicesBag bag)
        {
            var logger = bag.Logger;

            // 1. Parse Queue item first:
            var isValidDate = DateTime.TryParse(queueItem, out DateTime orderDate);
            if (!isValidDate)
                throw new Exception($"Could not convert the queue item '{queueItem}' into a valid DateTime object.");

            // 2. Call A&C API:
            try
            {
                await AbelAndColeForecastService.SendProductForecastAsync(
                    FuncName,
                    bag,
                    orderDate,
                    1,
                    true);
            }
            catch (Exception ex)
            {
                logger.LogError($"An unexpected error happened when trying to place the final order with Abel & Cole: {ex.Message}.");

                await bag.FedBot.SendMessage(TeamsCard.CreateError(ex, FuncName));
            }

            //AC 2020-03-18 These are now sent out at 2pm via a logic app
            // 3. Send Email to Seven Seeded:
            //try
            //{
            //    await BakeryOrderService.SendProductOrdersAsync(FuncName, bag, orderDate);
            //}
            //catch (Exception ex)
            //{
            //    logger.LogError($"An unexpected error happened when trying to place the final order with Seven Seeded: {ex.Message}.");

            //    await bag.FedBot.SendMessage(TeamsCard.CreateError(ex, FuncName));
            //}
        }
    }
}