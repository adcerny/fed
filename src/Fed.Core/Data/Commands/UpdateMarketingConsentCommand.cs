namespace Fed.Core.Data.Commands
{
    public class UpdateMarketingConsentCommand : IDataOperation<bool>
    {
        public UpdateMarketingConsentCommand(
            string emailAddress,
            bool isMarketingConsented)
        {
            EmailAddress = emailAddress;
            IsMarketingConsented = isMarketingConsented;
        }

        public string EmailAddress { get; }
        public bool IsMarketingConsented { get; }
    }
}