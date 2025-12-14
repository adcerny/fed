using Fed.Core.Entities;
using FluentValidation;

namespace Fed.Core.Services.Validators
{
    public class AddressValidator<T> : AbstractValidator<T> where T : Address
    {
        public AddressValidator()
        {

            RuleFor(address => address.CompanyName).NotEmpty().WithMessage("Please enter a company name");

            RuleFor(address => address.AddressLine1).NotEmpty().WithMessage("Please enter the first line of your address");

            //RuleFor(address => address.Town).NotEmpty().WithMessage("Please enter your town");
        }
    }
}
