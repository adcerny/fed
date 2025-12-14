using System;

namespace Fed.Core.Entities
{
    public class DeliverableAddress : Address
    {
        public DeliverableAddress()
        { }

        public DeliverableAddress(
            string companyName,
            string addressLine1,
            string addressLine2,
            string town,
            string postcode,
            Guid hubId)
            : base(companyName, addressLine1, addressLine2, town, postcode)
        {
            HubId = hubId;
        }

        public Guid HubId { get; set; }
    }
}