using Fed.Core.ValueTypes;
using System;

namespace Fed.Core.Data.Commands
{
    public class CreateOrderFromRecurringOrderCommand : IDataOperation<Guid>
    {
        public CreateOrderFromRecurringOrderCommand(Guid recurringOrderId, string shortId, Date deliveryDate, bool isFree)
        {
            RecurringOrderId = recurringOrderId;
            ShortId = shortId;
            DeliveryDate = deliveryDate;
            IsFree = isFree;
        }

        public Guid RecurringOrderId { get; }
        public string ShortId { get; }
        public Date DeliveryDate { get; }
        public bool IsFree { get; }
    }
}