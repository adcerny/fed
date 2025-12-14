using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Commands
{
    public class CreateDeliveryAddressCommand : IDataOperation<Guid>
    {
        public CreateDeliveryAddressCommand(Guid contactId, DeliveryAddress deliveryAddress)
        {
            ContactId = contactId;
            DeliveryAddress = deliveryAddress;
        }

        public Guid ContactId { get; }
        public DeliveryAddress DeliveryAddress { get; }
    }
}