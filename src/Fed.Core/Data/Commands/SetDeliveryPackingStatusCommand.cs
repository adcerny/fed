using Fed.Core.Enums;
using System;

namespace Fed.Core.Data.Commands
{
    public class SetDeliveryPackingStatusCommand : IDataOperation<bool>
    {
        public SetDeliveryPackingStatusCommand(Guid deliveryId, PackingStatus packingStatus)
        {
            DeliveryId = deliveryId;
            PackingStatus = packingStatus;
        }

        public Guid DeliveryId { get; }
        public PackingStatus PackingStatus { get; }
    }
}