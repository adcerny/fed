using Fed.Core.ValueTypes;
using System;

namespace Fed.Core.Data.Commands
{
    public class CreateOrderCommand : IDataOperation<Guid>
    {
        public CreateOrderCommand(String orderName, 
                                  Guid contactId, 
                                  Guid timeslotId, 
                                  int weeklyRecurrence, 
                                  Guid? recurringOrderId, 
                                  string shortId, 
                                  Date deliveryDate, 
                                  bool isFree)
        {
            OrderName = orderName;
            ContactId = contactId;
            TimeslotId = timeslotId;
            WeeklyRecurrence = weeklyRecurrence;
            RecurringOrderId = recurringOrderId;
            ShortId = shortId;
            DeliveryDate = deliveryDate;
            IsFree = isFree;
        }

        public string OrderName { get; }
        public Guid ContactId { get; }
        public Guid TimeslotId { get; }
        public int WeeklyRecurrence { get; }
        public Guid? RecurringOrderId { get; }
        public string ShortId { get; }
        public Date DeliveryDate { get; }
        public bool IsFree { get; }
    }
}