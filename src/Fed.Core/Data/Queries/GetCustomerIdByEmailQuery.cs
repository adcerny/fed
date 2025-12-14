using System;

namespace Fed.Core.Data.Queries
{
    public class GetCustomerIdByEmailQuery : IDataOperation<Guid?>
    {
        public GetCustomerIdByEmailQuery(string emailAddress)
        {
            Email = emailAddress;
        }

        public string Email { get; }
    }
}
