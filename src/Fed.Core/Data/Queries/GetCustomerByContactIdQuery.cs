using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Queries
{
    public class GetCustomerByContactIdQuery : IDataOperation<Customer>
    {
        public GetCustomerByContactIdQuery(Guid contactId)
        {
            ContactId = contactId;
        }

        public Guid ContactId { get; }
    }
}
