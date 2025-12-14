using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetHolidaysQuery : IDataOperation<IList<Holiday>>
    {
        public GetHolidaysQuery(DateRange dateRange)
        {
            DateRange = dateRange;
        }

        public DateRange DateRange { get; }
    }
}