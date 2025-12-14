using Fed.Core.Entities;
using Fed.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fed.Web.Portal.Models.Customers
{
    public class CreateCustomerModel
    {
        // Braintree
        // -----------------------------

        public string BraintreeClientToken { get; set; }
        public string BraintreeNonce { get; set; }
        public string CardholderName { get; set; }

        // General
        // -----------------------------

        public Guid CustomerId { get; set; }
        public Guid ContactId { get; set; }

        // Customer Type
        // -----------------------------

        [Required]
        public int AccountTypeId { get; set; }

        // Company
        // -----------------------------

        public string CompanyName { get; set; }
        public string OfficeSize { get; set; }
        public string Website { get; set; }
        public string Notes { get; set; }
        public string ACAccountNumber { get; set; }

        // Primary Contact
        // -----------------------------

        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
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

        // Payment Type
        // -----------------------------

        [Required]
        public int PaymentMethod { get; set; }

        // Billing Address
        // -----------------------------

        public string BillingPostCode { get; set; }
        public string BillingAddressLine1 { get; set; }
        public string BillingAddressLine2 { get; set; }
        public string BillingTown { get; set; }
        public string InvoiceReference { get; set; }

        // Billing Email
        // -----------------------------

        public string BillingEmail { get; set; }

        // Account Settings
        // -----------------------------

        public bool IsDeliveryChargeExempt { get; set; }
        public bool SplitDeliveriesByOrder { get; set; }
        public bool ExcludeFromInvoicing { get; set; }
        public bool IsTestAccount { get; set; }
        public bool IsFriend { get; set; }
        public bool SendPasswordResetEmail { get; set; }
        public bool MerchelloCustomerCreated { get; set; }

        public static CreateCustomerModel CreateEmpty() =>
            new CreateCustomerModel
            {
                CustomerId = Guid.Empty,
                AccountTypeId = (int)AccountType.Standard,
                CompanyName = "",
                OfficeSize = "",
                Website = "",
                Notes = "",
                ACAccountNumber = "",
                ContactFirstName = "",
                ContactLastName = "",
                ContactEmail = "",
                PhoneNumber = "",
                Source = "",
                MarketingConsent = false,
                DeliveryPostCode = "",
                DeliveryAddressLine1 = "",
                DeliveryAddressLine2 = "",
                DeliveryTown = "",
                DeliveryInstructions = "",
                DeliveryPhone = "",
                PaymentMethod = (int)Core.Enums.PaymentMethod.Card,
                BillingPostCode = "",
                BillingAddressLine1 = "",
                BillingAddressLine2 = "",
                BillingTown = "",
                InvoiceReference = "",
                BillingEmail = "",
                IsDeliveryChargeExempt = false,
                SplitDeliveriesByOrder = false,
                ExcludeFromInvoicing = false,
                IsTestAccount = false,
                IsFriend = false,
                SendPasswordResetEmail = false
            };

        public bool IsExistingCustomer => CustomerId != Guid.Empty;

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

        public void PatchCustomer(Customer customer, Guid? hubId = null)
        {
            // Initialise collections:

            if (customer.Contacts == null || customer.Contacts.Count == 0)
                customer.Contacts = new List<Contact> { Contact.CreateEmpty() };

            var contact = customer.PrimaryContact;

            if (contact.DeliveryAddresses == null || contact.DeliveryAddresses.Count == 0)
                contact.DeliveryAddresses = new List<DeliveryAddress> { DeliveryAddress.CreateEmpty(hubId ?? Guid.Empty) };

            var deliveryAddress = contact.GetPrimaryDeliveryAddress();

            if (contact.BillingAddresses == null || contact.BillingAddresses.Count == 0)
                contact.BillingAddresses = new List<BillingAddress> { BillingAddress.CreateEmpty() };

            var billingAddress = contact.GetPrimaryBillingAddress();

            // Prep Data:

            var fullName = $"{ContactFirstName} {ContactLastName}";
            var (officeSizeMin, officeSizeMax) = Customer.ParseOfficeSizeDescription(OfficeSize);

            // Update Entities:

            customer.AccountType = (AccountType)AccountTypeId;
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
            customer.CustomerMarketingAttributeId = MarketingAttributeId;

            contact.FirstName = ContactFirstName;
            contact.LastName = ContactLastName;
            contact.Email = ContactEmail;
            contact.Phone = PhoneNumber;
            contact.IsMarketingConsented = MarketingConsent;
            contact.PaymentMethod =
                Enum.IsDefined(typeof(PaymentMethod), (PaymentMethod)PaymentMethod)
                ? (PaymentMethod)PaymentMethod
                : Core.Enums.PaymentMethod.Card;

            deliveryAddress.FullName = fullName;
            deliveryAddress.CompanyName = CompanyName;
            deliveryAddress.AddressLine1 = DeliveryAddressLine1;
            deliveryAddress.AddressLine2 = DeliveryAddressLine2;
            deliveryAddress.Town = DeliveryTown;
            deliveryAddress.Postcode = DeliveryPostCode;
            deliveryAddress.DeliveryInstructions = DeliveryInstructions;
            deliveryAddress.Phone = DeliveryPhone;
            deliveryAddress.IsPrimary = true;

            billingAddress.FullName = fullName;
            billingAddress.CompanyName = CompanyName;
            billingAddress.AddressLine1 = BillingAddressLine1;
            billingAddress.AddressLine2 = BillingAddressLine2;
            billingAddress.Town = BillingTown;
            billingAddress.Postcode = BillingPostCode;
            billingAddress.Email = BillingEmail;
            billingAddress.Phone = PhoneNumber;
            billingAddress.InvoiceReference = InvoiceReference;
            billingAddress.IsPrimary = true;
        }

        public bool IsValidProspect(out List<string> errorMessages)
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

        public bool IsValidDeliverableCustomer(out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            IsValidProspect(out errorMessages);

            if (string.IsNullOrWhiteSpace(PhoneNumber))
                errorMessages.Add("You must enter a phone number before proceeding to step 2.");

            if (string.IsNullOrWhiteSpace(DeliveryPostCode))
                errorMessages.Add("You must enter a delivery postcode before proceeding to step 2.");

            if (string.IsNullOrWhiteSpace(DeliveryTown))
                errorMessages.Add("You must enter a delivery town before proceeding to step 2.");

            if (string.IsNullOrWhiteSpace(DeliveryAddressLine1))
                errorMessages.Add("You must enter a delivery address line 1 before proceeding to step 2.");

            return errorMessages.Count == 0;
        }

        public bool IsValidCustomer(out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            IsValidDeliverableCustomer(out errorMessages);

            if (string.IsNullOrWhiteSpace(BillingPostCode))
                errorMessages.Add("You must enter a billing postcode in order to create a customer.");

            if (string.IsNullOrWhiteSpace(BillingTown))
                errorMessages.Add("You must enter a billing town in order to create a customer.");

            if (string.IsNullOrWhiteSpace(BillingAddressLine1))
                errorMessages.Add("You must enter a billing address line 1 in order to create a customer.");

            if (string.IsNullOrWhiteSpace(BillingEmail))
                errorMessages.Add("You must enter a billing email in order to create a customer.");

            return errorMessages.Count == 0;
        }
    }
}