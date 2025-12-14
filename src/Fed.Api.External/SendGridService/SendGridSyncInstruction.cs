namespace Fed.Api.External.SendGridService
{
    public class SendGridSyncInstruction
    {
        public enum SyncSource
        {
            SendGrid,
            Fed
        }

        public enum SyncEvent
        {
            Unsubscribe,
            Subscribe,
            SetContact
        }

        public SyncSource Source { get; set; }
        public SyncEvent Event { get; set; }
        public string EmailAddress { get; set; }
        public SendGridContact Contact { get; set; }

        public string OldEmailAddress { get; set; }
    }
}
