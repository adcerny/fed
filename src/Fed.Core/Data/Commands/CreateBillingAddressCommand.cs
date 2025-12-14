using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Commands
{
    public class CreateBillingAddressCommand : IDataOperation<Guid>
    {
        public CreateBillingAddressCommand(Guid contactId, BillingAddress billingAddress)
        {
            ContactId = contactId;
            BillingAddress = billingAddress;
        }

        public Guid ContactId { get; }
        public BillingAddress BillingAddress { get; }
    }
}