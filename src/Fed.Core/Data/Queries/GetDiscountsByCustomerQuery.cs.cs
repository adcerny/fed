using Fed.Core.Entities;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetDiscountsByCustomerQuery : IDataOperation<IList<Discount>>
    {
        public GetDiscountsByCustomerQuery(Guid customerId, bool includeUnapplied = false)
        {
            CustomerId = customerId;
            IncludeUnapplied = includeUnapplied;
        }

        public Guid CustomerId { get; }

        public bool IncludeUnapplied { get; }
    }
}