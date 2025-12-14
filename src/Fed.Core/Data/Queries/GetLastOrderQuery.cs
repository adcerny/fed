using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Queries
{
    public class GetLastOrderQuery : IDataOperation<Order>
    {
        public GetLastOrderQuery(Guid recurringOrderId)
        {
            RecurringOrderId = recurringOrderId;
        }

        public Guid RecurringOrderId { get; }
    }
}