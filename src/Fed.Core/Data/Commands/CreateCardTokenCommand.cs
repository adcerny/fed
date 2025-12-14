using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Commands
{
    public class CreateCardTokenCommand : IDataOperation<Guid>
    {
        public CreateCardTokenCommand(Guid contactId, CardToken cardToken)
        {
            ContactId = contactId;
            CardToken = cardToken;
        }

        public Guid ContactId { get; }
        public CardToken CardToken { get; }
    }
}