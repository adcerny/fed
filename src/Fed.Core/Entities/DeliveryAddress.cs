using System;

namespace Fed.Core.Entities
{
    public class DeliveryAddress : DeliverableAddress
    {
        public DeliveryAddress(
            Guid id,
            Guid contactId,
            bool isPrimary,
            string fullName,
            string companyName,
            string addressLine1,
            string addressLine2,
            string town,
            string postcode,
            string deliveryInstructions,
            string phone,
            bool leaveDeliveryOutside,
            Guid hubId)
            : base(companyName, addressLine1, addressLine2, town, postcode, hubId)
        {
            Id = id;
            ContactId = contactId;
            IsPrimary = isPrimary;
            FullName = fullName;
            DeliveryInstructions = deliveryInstructions;
            LeaveDeliveryOutside = leaveDeliveryOutside;
            Phone = phone;
        }

        public Guid Id { get; }
        public Guid ContactId { get; }
        public bool IsPrimary { get; set; }
        public string FullName { get; set; }
        public string DeliveryInstructions { get; set; }
        public string Phone { get; set; }
        public bool LeaveDeliveryOutside { get; set; }


        public static DeliveryAddress CreateEmpty(Guid hubId) =>
            new DeliveryAddress(
                Guid.Empty,     // Delivery Address ID
                Guid.Empty,     // Contact ID
                true,           // Is Primary Address
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                false,          // Leave Delivery Outside
                hubId);
    }
}