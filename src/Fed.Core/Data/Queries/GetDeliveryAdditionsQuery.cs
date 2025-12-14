using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetDeliveryAdditionsQuery : IDataOperation<IList<DeliveryShortage>>
    {
        public GetDeliveryAdditionsQuery(DateRange dateRange)
        {
            DateRange = dateRange;
        }

        public DateRange DateRange { get; }
    }
}