using Fed.Core.Entities;
using FluentValidation;

namespace Fed.Core.Services.Validators
{
    public class DeliveryAddressValidator : AddressValidator<DeliveryAddress>
    {
        public DeliveryAddressValidator()
        {
            RuleFor(d => d.FullName).NotEmpty().WithMessage("Please enter a name");

            RuleFor(address => address.Postcode).NotEmpty().WithMessage("Please enter your postcode")
                                        .Matches(PostcodeLocation.RegEx)
                                        .WithMessage("Please enter a valid postcode");
        }
    }
}
