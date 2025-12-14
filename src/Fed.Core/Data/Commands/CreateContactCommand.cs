using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Commands
{
    public class CreateContactCommand : IDataOperation<Guid>
    {
        public CreateContactCommand(Guid customerId, Contact contact)
        {
            CustomerId = customerId;
            Contact = contact;
        }

        public Guid CustomerId { get; }
        public Contact Contact { get; }
    }
}