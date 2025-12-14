using System;

namespace Fed.Api.External.ActivityLogs
{
    public class CustomerActivity
    {
        public string UserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public object Metadata { get; set; }
        public string PartitionKey { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
