using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetOrderSummaryQuery : IDataOperation<IList<OrderSummary>>
    {
        public GetOrderSummaryQuery(DateRange dateRange, Guid contactId)
        {
            DateRange = dateRange;
            ContactId = contactId;
        }

        public DateRange DateRange { get; }
        public Guid ContactId { get; }
    }
}