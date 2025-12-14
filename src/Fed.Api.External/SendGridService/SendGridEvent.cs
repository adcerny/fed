using System;

namespace Fed.Api.External.SendGridService
{
    public class SendGridEvent
    {
        public string Email { get; set; }
        public long Timestamp { get; set; }
        public string Event { get; set; }

        public bool IsUnsubscribed =>
            Event.Equals("unsubscribe", StringComparison.OrdinalIgnoreCase)
            || Event.Equals("group_unsubscribe", StringComparison.OrdinalIgnoreCase);

        public bool IsResubscribed =>
            Event.Equals("group_resubscribe", StringComparison.OrdinalIgnoreCase);
    }
}
