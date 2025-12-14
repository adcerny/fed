using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace Fed.AzureFunctions.Entities
{
    public class CustomerActivityEntity : TableEntity
    {
        public CustomerActivityEntity(ActivityType activityType, string userId, string ipAddress, string userAgent, string metadata) : base(activityType.ToString(), Guid.NewGuid().ToString())
        {
            UserId = userId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            Metadata = metadata;
        }

        public CustomerActivityEntity() : base()
        {
        }
        public string UserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Metadata { get; set; }
    }

    public enum ActivityType
    {
        Logon = 1,
        AutoLogon = 2,
        Search = 3,
        PostcodeLookup = 4
    }
}
