using Fed.Core.Entities;
using Fed.Core.Enums;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetDiscountsApplicableQuery : IDataOperation<IList<Discount>>
    {
        public GetDiscountsApplicableQuery(DiscountEvent appliedEvent)
        {
            AppliedEvent = appliedEvent;
        }

        public DiscountEvent AppliedEvent { get; }
    }
}