using Fed.Core.Entities;
using System.Linq;

namespace Fed.Web.Portal.Models.Customers
{
    public class CardDetailsModel
    {
        public CardDetailsModel(
            Customer customer,
            string clientToken)
        {
            Customer = customer;
            Contact = customer.Contacts.FirstOrDefault();
            BillingAddress = Contact?.BillingAddresses?.FirstOrDefault();
            ClientToken = clientToken;
        }

        public Contact Contact { get; }
        public Customer Customer { get; }
        public BillingAddress BillingAddress { get; }
        public string ClientToken { get; }
    }
}
