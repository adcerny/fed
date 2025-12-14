using Fed.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fed.Core.Entities
{
    public class Customer
    {
        private IList<Contact> _contacts;

        public Customer(
            Guid id,
            string shortId,
            string companyName,
            string website,
            int? acAccountNumber,
            bool? isInvoiceable,
            int? officeSizeMin,
            int? officeSizeMax,
            bool? isDeliveryChargeExempt,
            bool? splitDeliveriesByOrder,
            bool isTestAccount,
            AccountType accountTypeId,
            string source,
            string notes,
            bool isFriend,
            string cancellationReason,
            DateTime? registerDate = null,
            DateTime? firstDeliveryDate = null,
            DateTime? lastDeliveryDate = null,
            Guid? customerAgentId = null,
            CustomerAgent customerAgent = null,
             Guid? customerMarketingAttributeId = null,
            CustomerMarketingAttribute customerMarketingAttribute = null)
        {
            Id = id;
            ShortId = shortId;
            CompanyName = companyName;
            Website = website;
            ACAccountNumber = acAccountNumber;
            Contacts = new List<Contact>();
            IsInvoiceable = isInvoiceable ?? true;
            OfficeSizeMin = officeSizeMin;
            OfficeSizeMax = officeSizeMax;
            IsDeliveryChargeExempt = isDeliveryChargeExempt ?? false;
            SplitDeliveriesByOrder = splitDeliveriesByOrder ?? false;
            IsTestAccount = isTestAccount;
            IsFriend = isFriend;
            AccountType = accountTypeId;
            Source = source;
            Notes = notes;
            CancellationReason = cancellationReason;
            RegisterDate = registerDate ?? DateTime.UtcNow;
            FirstDeliveryDate = firstDeliveryDate;
            LastDeliveryDate = lastDeliveryDate;
            CustomerAgentId = customerAgentId;
            CustomerAgent = customerAgent;
            CustomerMarketingAttributeId = customerMarketingAttributeId;
            CustomerMarketingAttribute = customerMarketingAttribute;
        }

        public Guid Id { get; }
        public string ShortId { get; }
        public string CompanyName { get; set; }
        public string Website { get; set; }
        public int? ACAccountNumber { get; set; }
        public bool IsInvoiceable { get; set; }
        public int? OfficeSizeMin { get; set; }
        public int? OfficeSizeMax { get; set; }
        public bool IsDeliveryChargeExempt { get; set; }
        public bool SplitDeliveriesByOrder { get; set; }
        public bool IsTestAccount { get; set; }
        public bool IsFriend { get; set; }
        public AccountType AccountType { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }
        public string CancellationReason { get; set; }

        public LifecycleStatus LifecycleStatus
        {
            get
            {
                if (!LastDeliveryDate.HasValue)
                    return LifecycleStatus.Prospect;

                return
                    ((DateTime.Today - LastDeliveryDate.Value).TotalDays < 30)
                    ? LifecycleStatus.Active
                    : LifecycleStatus.Lapsed;
            }
        }

        public DateTime RegisterDate { get; }
        public DateTime? FirstDeliveryDate { get; }
        public DateTime? LastDeliveryDate { get; }

        public IList<Contact> Contacts
        {
            get { return _contacts; }
            set
            {
                _contacts = value;

                if (value != null && value.Count > 0)
                    PrimaryContact = value.First();
            }
        }

        public Guid? CustomerAgentId { get; set; }
        public CustomerAgent CustomerAgent { get; set; }
        public Guid? CustomerMarketingAttributeId { get; set; }
        public CustomerMarketingAttribute CustomerMarketingAttribute { get; set; }

        public Contact PrimaryContact
        {
            get;
            private set;
        }

        public string GetOfficeSizeDescription()
        {
            if (OfficeSizeMin.HasValue && OfficeSizeMax.HasValue)
                return $"{OfficeSizeMin} - {OfficeSizeMax}";
            if (OfficeSizeMin.HasValue && !OfficeSizeMax.HasValue)
                return $"{OfficeSizeMin} +";
            if (!OfficeSizeMin.HasValue && OfficeSizeMax.HasValue)
                return $"{OfficeSizeMax}";

            return string.Empty;
        }

        public static (int? minOfficeSize, int? maxOfficeSize) ParseOfficeSizeDescription(string officeSizeDescription)
        {
            try
            {
                if (string.IsNullOrEmpty(officeSizeDescription))
                    return (null, null);

                // Split on one or more non-digit characters.
                var split = Regex.Split(officeSizeDescription, @"\D+");
                var numbers = new List<int?>();

                foreach (string value in split)
                {
                    if (!string.IsNullOrEmpty(value))
                        numbers.Add(int.Parse(value));
                }

                if (numbers.Count == 1)
                {
                    //this is a min value
                    if (officeSizeDescription.Contains('+'))
                        return (numbers[0], null);
                    else
                        return (null, numbers[0]);
                }

                if (numbers.Count >= 2)
                    return (numbers[0], numbers[1]);

                return (null, null);
            }
            catch
            {
                return (null, null);
            }
        }
    }
}