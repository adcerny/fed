using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetSkipDatesQuery : IDataOperation<IList<SkipDate>>
    {
        public GetSkipDatesQuery(Guid? recurringOrderId, DateRange? dateRange)
        {
            RecurringOrderId = recurringOrderId;
            DateRange = dateRange;
        }

        public Guid? RecurringOrderId { get; }

        public DateRange? DateRange { get; }
    }
}