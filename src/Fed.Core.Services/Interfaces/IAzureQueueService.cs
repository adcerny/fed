using System.Threading.Tasks;

namespace Fed.Api.External.AzureStorage
{
    public interface IAzureQueueService
    {
        Task SyncSubscriberWithSendGridAsync(string emailAddress, bool isSubscribedToMarketingEmails);

        Task SyncContactWithSendGridAsync(
            string oldEmailAddress,
            string emailAddress,
            string customerShortId,
            string contactShortId,
            string companyName,
            string firstName,
            string lastName);
    }
}