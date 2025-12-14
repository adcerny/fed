using Fed.Core.Entities;
using Fed.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fed.Web.Portal.Models.Customers
{
    public class AccountInfoModel
    {
        // General
        // -----------------------------

        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public Guid ContactId { get; set; }
        [Required]
        public Guid DeliveryAddressId { get; set; }

        public LifecycleStatus LifecycleStatus { get; set; }
        public AccountType AccountType { get; set; }

        // Customer Type
        // -----------------------------

        [Required]
        public int AccountTypeId { get; set; }
        public string CancellationReason { get; set; }

        // Company
        // -----------------------------

        [Required]
        public string CompanyName { get; set; }
        public string OfficeSize { get; set; }
        public string Website { get; set; }
        public string Notes { get; set; }
        public string ACAccountNumber { get; set; }


        // Primary Contact
        // -----------------------------

        [Required]
        public string ContactFirstName { get; set; }
        [Required]
        public string ContactLastName { get; set; }
        [Required]
        public string ContactEmail { get; set; }

        public string PhoneNumber { get; set; }

        // Marketing Information
        // -----------------------------

        public string Source { get; set; }
        public bool MarketingConsent { get; set; }

        public Guid? MarketingAttributeId { get; set; }

        public IList<CustomerMarketingAttribute> MarketingAttributes { get; set; }
        // Delivery Address
        // -----------------------------

        public string DeliveryPostCode { get; set; }
        public string DeliveryAddressLine1 { get; set; }
        public string DeliveryAddressLine2 { get; set; }
        public string DeliveryTown { get; set; }
        public string DeliveryInstructions { get; set; }
        public string DeliveryPhone { get; set; }

        // Account Settings
        // -----------------------------

        public bool IsDeliveryChargeExempt { get; set; }
        public bool SplitDeliveriesByOrder { get; set; }
        public bool ExcludeFromInvoicing { get; set; }
        public bool IsTestAccount { get; set; }
        public bool IsFriend { get; set; }
        public string IsDirty { get; set; }

        public bool HasPartialDeliveryDetails =>
            !string.IsNullOrEmpty(DeliveryPostCode)
            || !string.IsNullOrEmpty(DeliveryTown)
            || !string.IsNullOrEmpty(DeliveryAddressLine1)
            || !string.IsNullOrEmpty(DeliveryAddressLine2)
            || !string.IsNullOrEmpty(DeliveryInstructions)
            || !string.IsNullOrEmpty(DeliveryPhone);

        public bool HasMandatoryDeliveryDetails =>
            !string.IsNullOrEmpty(DeliveryPostCode)
            && !string.IsNullOrEmpty(DeliveryTown)
            && !string.IsNullOrEmpty(DeliveryAddressLine1);

        public bool IsValidData(out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(CompanyName))
                errorMessages.Add("You must enter a company name before saving a customer.");

            if (string.IsNullOrWhiteSpace(ContactFirstName))
                errorMessages.Add("You must enter a first name before saving a customer.");

            if (string.IsNullOrWhiteSpace(ContactLastName))
                errorMessages.Add("You must enter a last name before saving a customer.");

            if (string.IsNullOrWhiteSpace(ContactEmail))
                errorMessages.Add("You must enter an email address before saving a customer.");

            if (HasPartialDeliveryDetails && !HasMandatoryDeliveryDetails)
                errorMessages.Add("In order to save delivery details you must provide at least a postcode, town and the first line of address.");

            return errorMessages.Count == 0;
        }

        public static AccountInfoModel FromCustomer(FullCustomerInfo customer)
        {
            var deliveryAddress = customer.PrimaryContact.GetPrimaryDeliveryAddress();

            if (deliveryAddress == null)
                deliveryAddress = DeliveryAddress.CreateEmpty(Guid.Empty);

            return new AccountInfoModel
            {
                CustomerId = customer.Id,
                ContactId = customer.PrimaryContact.Id,
                DeliveryAddressId = deliveryAddress.Id,
                AccountType = customer.AccountType,
                LifecycleStatus = customer.LifecycleStatus,
                AccountTypeId = (int)customer.AccountType,
                CancellationReason = customer.CancellationReason,
                CompanyName = customer.CompanyName,
                OfficeSize = customer.GetOfficeSizeDescription(),
                Website = customer.Website,
                Notes = customer.Notes,
                ACAccountNumber = customer.ACAccountNumber.HasValue ? customer.ACAccountNumber.ToString() : string.Empty,
                ContactFirstName = customer.PrimaryContact.FirstName,
                ContactLastName = customer.PrimaryContact.LastName,
                ContactEmail = customer.PrimaryContact.Email,
                PhoneNumber = customer.PrimaryContact.Phone,
                Source = customer.Source,
                MarketingConsent = customer.PrimaryContact.IsMarketingConsented,
                MarketingAttributeId = customer.CustomerMarketingAttributeId,
                DeliveryPostCode = deliveryAddress.Postcode,
                DeliveryAddressLine1 = deliveryAddress.AddressLine1,
                DeliveryAddressLine2 = deliveryAddress.AddressLine2,
                DeliveryTown = deliveryAddress.Town,
                DeliveryInstructions = deliveryAddress.DeliveryInstructions,
                DeliveryPhone = deliveryAddress.Phone,
                IsDeliveryChargeExempt = customer.IsDeliveryChargeExempt,
                SplitDeliveriesByOrder = customer.SplitDeliveriesByOrder,
                ExcludeFromInvoicing = !customer.IsInvoiceable,
                IsTestAccount = customer.IsTestAccount,
                IsFriend = customer.IsFriend
            };
        }

        public void PatchCustomer(Customer customer)
        {
            // Initialise objects:

            var contact = customer.PrimaryContact;
            var deliveryAddress = contact.GetPrimaryDeliveryAddress();

            // Prep Data:

            var fullName = $"{ContactFirstName} {ContactLastName}";
            var (officeSizeMin, officeSizeMax) = Customer.ParseOfficeSizeDescription(OfficeSize);

            // Update Entities:

            customer.AccountType = (AccountType)AccountTypeId;
            customer.CancellationReason = CancellationReason;
            customer.CompanyName = CompanyName;
            customer.OfficeSizeMin = officeSizeMin;
            customer.OfficeSizeMax = officeSizeMax;
            customer.Website = Website;
            customer.Notes = Notes;
            customer.ACAccountNumber = string.IsNullOrEmpty(ACAccountNumber) ? (int?)null : int.Parse(ACAccountNumber);
            customer.Source = Source;
            customer.IsInvoiceable = !ExcludeFromInvoicing;
            customer.IsDeliveryChargeExempt = IsDeliveryChargeExempt;
            customer.SplitDeliveriesByOrder = SplitDeliveriesByOrder;
            customer.IsTestAccount = IsTestAccount;
            customer.IsFriend = IsFriend;

            contact.FirstName = ContactFirstName;
            contact.LastName = ContactLastName;
            contact.Email = ContactEmail;
            contact.Phone = PhoneNumber;
            contact.IsMarketingConsented = MarketingConsent;

            deliveryAddress.FullName = fullName;
            deliveryAddress.CompanyName = CompanyName;
            deliveryAddress.AddressLine1 = DeliveryAddressLine1;
            deliveryAddress.AddressLine2 = DeliveryAddressLine2;
            deliveryAddress.Town = DeliveryTown;
            deliveryAddress.Postcode = DeliveryPostCode;
            deliveryAddress.DeliveryInstructions = DeliveryInstructions;
            deliveryAddress.Phone = DeliveryPhone;
        }
    }
}