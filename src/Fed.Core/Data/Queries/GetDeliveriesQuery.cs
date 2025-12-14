using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetDeliveriesQuery : IDataOperation<IList<Delivery>>
    {
        public GetDeliveriesQuery(DateRange dateRange)
        {
            DateRange = dateRange;
        }

        public DateRange DateRange { get; }
    }
}