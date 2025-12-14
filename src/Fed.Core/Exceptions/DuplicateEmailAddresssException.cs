using Fed.Core.Entities;
using System;

namespace Fed.Core.Exceptions
{
    public class DuplicateEmailAddresssException : FedException
    {
        public DuplicateEmailAddresssException(string emailAddress, Guid customerId)
            : base(
                  ErrorCode.DuplicateEmailAddress,
                  $"The email address '{emailAddress}' is already taken.")
        {
            ExistingCustomerId = customerId;
        }

        public Guid ExistingCustomerId { get; set; }
    }
}
