using Fed.Core.Entities;
using Fed.Core.ValueTypes;

namespace Fed.Web.Portal.ViewModels
{
    public class AccountsCreatedDownloadViewModel
    {
        private Customer _customer;
        private Date? _nextDeliveryDate;


        public AccountsCreatedDownloadViewModel(Customer customer, Date? nextDeliveryDate)
        {
            _customer = customer;
            _nextDeliveryDate = nextDeliveryDate;
        }

        public string CustomerShortId { get { return _customer.ShortId; } }

        public string PortalUrl { get { return $"https://portal.fedteam.co.uk/customers/{_customer.ShortId}"; } }

        public string CompanyName { get { return _customer.CompanyName; } }

        public string AccountType { get { return _customer.AccountType.ToString(); } }

        public string OfficeSize { get { return _customer.GetOfficeSizeDescription(); } }

        public string Source { get { return _customer.Source; } }

        public string Email { get { return _customer.PrimaryContact.Email; } }

        public bool OptIn { get { return _customer.PrimaryContact.IsMarketingConsented; } }

        public string RegisterDate { get { return _customer.RegisterDate.ToString("yyyy-MM-dd"); } }

        public string AccountStatus { get { return _customer.LifecycleStatus.ToString(); } }

        public string AccountSetup { get { return _customer.PrimaryContact.GetPrimaryBillingAddress() != null ? "Complete" : null; } }

        public string FirstDeliveryDate { get { return _customer.FirstDeliveryDate?.ToString("yyyy-MM-dd"); } }

        public string NextDeliveryDate { get { return _nextDeliveryDate.ToString(); } }

        public string Agent { get { return _customer.CustomerAgent?.Initials; } }

        public string MarketingAttribute { get { return _customer.CustomerMarketingAttribute?.Name; } }
    }
}
