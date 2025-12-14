using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Common;
using Fed.Core.Data.Commands;
using Fed.Infrastructure.Data.SqlServer.Handlers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class SendGridSyncWorkerFunction
    {
        private const string FuncName = "SendGridSyncWorkerFunction";

        [FunctionName(FuncName)]
        public static Task Run(
            [QueueTrigger(QueueNames.SendGridSync)]string queueItem,
            ILogger logger)
            => FunctionRunner.RunWithArgAsync(
                logger,
                FuncName,
                queueItem,
                SyncUnsubscribers,
                (ex) => throw new Exception("An error happended during syncing (un)subscribers between Fed and SendGrid", ex));

        public static async Task SyncUnsubscribers(string queueItem, ServicesBag bag)
        {
            var logger = bag.Logger;
            logger.LogInformation($"Received instruction to sync between Fed and SendGrid: {queueItem}");

            var syncInstruction = JsonConvert.DeserializeObject<SendGridSyncInstruction>(queueItem);

            logger.LogInformation($"Email Address to sync: {syncInstruction.EmailAddress}");
            logger.LogInformation($"Sync Source: {syncInstruction.Source}");
            logger.LogInformation($"Sync Event: {syncInstruction.Event}");

            logger.LogInformation("Initialising contacts handler...");
            var contactsHandler = new ContactsHandler(bag.SqlConfig, new RandomIdGenerator());

            if (syncInstruction.Source == SendGridSyncInstruction.SyncSource.Fed)
            {
                logger.LogInformation("Sending HTTP POST to SendGrid to update the customer's marketing consent...");

                if (syncInstruction.Event == SendGridSyncInstruction.SyncEvent.Unsubscribe)
                    await bag.SendGridService.UnsubscribeFromGroupAsync(
                        syncInstruction.EmailAddress,
                        bag.Config.SendGridMarketingGroupId);
                else if (syncInstruction.Event == SendGridSyncInstruction.SyncEvent.Subscribe)
                    await bag.SendGridService.RemoveSupressionFromGroupAsync(
                        syncInstruction.EmailAddress,
                        bag.Config.SendGridMarketingGroupId);
                else if (syncInstruction.Event == SendGridSyncInstruction.SyncEvent.SetContact)
                    await bag.SendGridService.AddOrUpdateContact(syncInstruction.OldEmailAddress,
                        syncInstruction.EmailAddress,
                        syncInstruction.Contact);
                else
                    throw new NotSupportedException($"The event {syncInstruction.Event} is not supported by the sync service.");
            }
            else
            {
                logger.LogInformation("Updating Fed database to set the customer's marketing consent...");

                var isMarketingConsented =
                    syncInstruction.Event == SendGridSyncInstruction.SyncEvent.Subscribe;

                await contactsHandler.ExecuteAsync(
                    new UpdateMarketingConsentCommand(
                        syncInstruction.EmailAddress,
                        isMarketingConsented));
            }

            logger.LogInformation("Sync complete");
        }
    }
}