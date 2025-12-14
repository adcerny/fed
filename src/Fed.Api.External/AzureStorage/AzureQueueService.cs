using Fed.Api.External.SendGridService;
using Fed.Core.Common;
using Fed.Core.Common.Interfaces;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Fed.Api.External.AzureStorage
{
    public class AzureQueueService : IAzureQueueService
    {
        private readonly IAzureConfig _azureConfig;

        public AzureQueueService(IAzureConfig azureConfig)
        {
            _azureConfig = azureConfig ?? throw new ArgumentNullException(nameof(azureConfig));
        }

        public Task SyncSubscriberWithSendGridAsync(string emailAddress, bool isSubscribedToMarketingEmails)
        {
            var syncInstruction = new SendGridSyncInstruction
            {
                EmailAddress = emailAddress.ToLower(),
                Event =
                        isSubscribedToMarketingEmails
                            ? SendGridSyncInstruction.SyncEvent.Subscribe
                            : SendGridSyncInstruction.SyncEvent.Unsubscribe,
                Source = SendGridSyncInstruction.SyncSource.Fed
            };

            var storageAccount = CloudStorageAccount.Parse(_azureConfig.StorageConnectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("sendgrid-sync");
            return queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(syncInstruction)));
        }

        public Task SyncContactWithSendGridAsync(
            string oldEmailAddress,
            string emailAddress,
            string customerShortId,
            string contactShortId,
            string companyName,
            string firstName,
            string lastName)
        {
            var sendGridContact =
                new SendGridContact
                {
                    CompanyId = customerShortId,
                    ContactId = contactShortId,
                    CompanyName = companyName,
                    FirstName = firstName,
                    LastName = lastName
                };

            var storageAccount = CloudStorageAccount.Parse(_azureConfig.StorageConnectionString);
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference("sendgrid-sync");

            var syncInstruction = new SendGridSyncInstruction
            {
                EmailAddress = emailAddress.ToLower(),
                Event = SendGridSyncInstruction.SyncEvent.SetContact,
                Source = SendGridSyncInstruction.SyncSource.Fed,
                Contact = sendGridContact,
                OldEmailAddress = oldEmailAddress.ToLower()
            };

            return queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(syncInstruction)));
        }
    }
}