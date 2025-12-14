using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetDeliveryShortagesQuery : IDataOperation<IList<DeliveryShortage>>
    {
        public GetDeliveryShortagesQuery(DateRange dateRange)
        {
            DateRange = dateRange;
        }

        public DateRange DateRange { get; }
    }
}