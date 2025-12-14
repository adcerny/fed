using Fed.Core.Entities;
using Fed.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Fed.Web.Portal.Models.Customers
{
    public class PaymentInfoModel
    {
        // General
        // -----------------------------

        [Required]
        public Guid CustomerId { get; set; }
        [Required]
        public Guid ContactId { get; set; }
        [Required]
        public Guid BillingAddressId { get; set; }

        public LifecycleStatus LifecycleStatus { get; set; }
        public AccountType AccountType { get; set; }

        public string ContactEmail { get; set; }
        public string DeliveryPostCode { get; set; }
        public string DeliveryAddressLine1 { get; set; }
        public string DeliveryAddressLine2 { get; set; }
        public string DeliveryTown { get; set; }

        // Payment Type
        // -----------------------------

        [Required]
        public int PaymentMethod { get; set; }

        public CardToken CardDetails { get; set; }

        // Billing Address
        // -----------------------------

        [Required]
        public string BillingCompanyName { get; set; }
        [Required]
        public string BillingPostCode { get; set; }
        [Required]
        public string BillingAddressLine1 { get; set; }
        public string BillingAddressLine2 { get; set; }
        [Required]
        public string BillingTown { get; set; }
        public string InvoiceReference { get; set; }

        // Billing Email
        // -----------------------------

        public string BillingEmail { get; set; }

        // Methods
        // -----------------------------

        public static PaymentInfoModel FromCustomer(FullCustomerInfo customer)
        {
            var billingAddress = customer.PrimaryContact.GetPrimaryBillingAddress();

            if (billingAddress == null)
                billingAddress = BillingAddress.CreateEmpty();

            var deliveryAddress = customer.PrimaryContact.GetPrimaryDeliveryAddress();

            if (deliveryAddress == null)
                deliveryAddress = DeliveryAddress.CreateEmpty(Guid.Empty);

            return new PaymentInfoModel
            {
                CustomerId = customer.Id,
                ContactId = customer.PrimaryContact.Id,
                BillingAddressId = billingAddress.Id,
                BillingCompanyName = !string.IsNullOrEmpty(billingAddress.CompanyName) ? billingAddress.CompanyName : customer.CompanyName,
                LifecycleStatus = customer.LifecycleStatus,
                AccountType = customer.AccountType,
                DeliveryPostCode = deliveryAddress.Postcode,
                DeliveryAddressLine1 = deliveryAddress.AddressLine1,
                DeliveryAddressLine2 = deliveryAddress.AddressLine2,
                DeliveryTown = deliveryAddress.Town,
                ContactEmail = customer.PrimaryContact.Email,
                PaymentMethod = (int)customer.PrimaryContact.PaymentMethod,
                CardDetails = customer.PrimaryContact.CardTokens?.First(c => c.IsPrimary),
                BillingPostCode = billingAddress.Postcode,
                BillingAddressLine1 = billingAddress.AddressLine1,
                BillingAddressLine2 = billingAddress.AddressLine2,
                BillingTown = billingAddress.Town,
                InvoiceReference = billingAddress.InvoiceReference,
                BillingEmail = billingAddress.Email
            };
        }

        public void PatchCustomer(Customer customer)
        {
            // Initialise objects:

            var contact = customer.PrimaryContact;
            var billingAddress = contact.GetPrimaryBillingAddress();

            // Update Entities:

            contact.PaymentMethod = (PaymentMethod)PaymentMethod;
            billingAddress.CompanyName = BillingCompanyName;
            billingAddress.FullName = $"{contact.FirstName} {contact.LastName}";
            billingAddress.Postcode = BillingPostCode;
            billingAddress.AddressLine1 = BillingAddressLine1;
            billingAddress.AddressLine2 = BillingAddressLine2;
            billingAddress.Town = BillingTown;
            billingAddress.InvoiceReference = InvoiceReference;
            billingAddress.Email = BillingEmail;
        }
    }
}