using Fed.Core.Entities;
using FluentValidation;

namespace Fed.Core.Services.Validators
{
    public class ContactValidator : AbstractValidator<Contact>
    {
        public ContactValidator()
        {
            RuleFor(contact => contact.FirstName).NotEmpty().WithMessage("Please enter a first name");

            RuleFor(contact => contact.LastName).NotEmpty().WithMessage("Please enter a last name");

            RuleFor(contact => contact.Email).NotEmpty().WithMessage("Please enter an email address")
                                             .EmailAddress()
                                             .WithMessage("Please enter a valid email address");

            //RuleFor(contact => contact.Phone).NotEmpty().WithMessage("Please enter a phone number")
            //                                 .Length(7, 30).WithMessage("Please eneter a valid phone number");

            RuleForEach(contact => contact.DeliveryAddresses).SetValidator(new DeliveryAddressValidator());
            RuleForEach(c => c.BillingAddresses).SetValidator(new BillingAddressValidator());

            // Can have only one primary delivery address
            // Can have only one primary billing address

            // Can have only one billing address
            // Can have only one card token


        }
    }
}
