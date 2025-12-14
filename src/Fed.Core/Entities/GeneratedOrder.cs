using System;

namespace Fed.Core.Entities
{
    public class GeneratedOrder
    {
        public GeneratedOrder(
            Guid recurringOrderId,
            Guid generatedOrderId,
            string shortOrderId)
        {
            RecurringOrderId = recurringOrderId;
            GeneratedOrderId = generatedOrderId;
            ShortOrderId = shortOrderId;
        }

        public Guid RecurringOrderId { get; }
        public Guid GeneratedOrderId { get; }
        public string ShortOrderId { get; }
    }
}