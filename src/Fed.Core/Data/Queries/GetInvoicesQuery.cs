using Fed.Core.Entities;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetInvoicesQuery : IDataOperation<IList<Invoice>>
    {
        public GetInvoicesQuery(Guid contactId)
        {
            ContactId = contactId;
        }

        public Guid ContactId { get; }
    }
}