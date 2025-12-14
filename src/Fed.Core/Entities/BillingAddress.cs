using System;

namespace Fed.Core.Entities
{
    public class BillingAddress : Address
    {
        public BillingAddress(
            Guid id,
            Guid contactId,
            bool isPrimary,
            string fullName,
            string companyName,
            string addressLine1,
            string addressLine2,
            string town,
            string postcode,
            string email,
            string phone,
            string invoiceReference)
            : base(companyName, addressLine1, addressLine2, town, postcode)
        {
            Id = id;
            ContactId = contactId;
            IsPrimary = isPrimary;
            FullName = fullName;
            Email = email?.Trim();
            Phone = phone;
            InvoiceReference = invoiceReference;
        }

        public Guid Id { get; }
        public Guid ContactId { get; }
        public bool IsPrimary { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string InvoiceReference { get; set; }

        public static BillingAddress CreateEmpty() =>
            new BillingAddress(
                Guid.Empty,     // Billing Address ID
                Guid.Empty,     // Contact ID
                true,           // Is Primary Address
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "");
    }
}