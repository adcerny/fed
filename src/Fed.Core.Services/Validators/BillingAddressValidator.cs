using Fed.Core.Entities;
using FluentValidation;

namespace Fed.Core.Services.Validators
{
    public class BillingAddressValidator : AddressValidator<BillingAddress>
    {
        public BillingAddressValidator()
        {
            RuleFor(b => b.FullName).NotEmpty().WithMessage("Please enter a name");

            RuleFor(b => b.Email).EmailAddress().WithMessage("Please enter a valid email address");
        }
    }
}
