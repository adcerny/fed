using Fed.Core.Entities;
using FluentValidation;

namespace Fed.Core.Services.Validators
{
    public class CustomerValidator : AbstractValidator<Customer>
    {
        public CustomerValidator()
        {
            RuleFor(customer => customer.CompanyName).NotEmpty().WithMessage("Please enter a company name");
            RuleForEach(customer => customer.Contacts).SetValidator(new ContactValidator());
        }
    }
}
