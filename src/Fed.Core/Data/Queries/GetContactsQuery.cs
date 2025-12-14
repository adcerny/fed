using Fed.Core.Entities;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetContactsQuery : IDataOperation<IList<Contact>>
    {
        public GetContactsQuery(Guid customerId)
        {
            CustomerId = customerId;
        }

        public Guid CustomerId { get; }
    }
}
