using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetCustomersQuery : IDataOperation<IList<Customer>>
    {
        public GetCustomersQuery(bool includeContacts)
        {
            IncludeContacts = includeContacts;
        }

        public bool IncludeContacts { get; }
    }
}
