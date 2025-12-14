using System;

namespace Fed.Core.Entities
{
    public class CardToken
    {
        public CardToken(
            Guid id,
            Guid contactId,
            int expiresMonth,
            int expiresYear,
            string obscuredCardNumber,
            string cardHolderFullName,
            string addressLine1,
            string postcode,
            bool isPrimary,
            DateTime? createdDate = null)
        {
            Id = id;
            ContactId = contactId;
            ExpiresMonth = expiresMonth;
            ExpiresYear = expiresYear;
            ObscuredCardNumber = obscuredCardNumber;
            CardHolderFullName = cardHolderFullName;
            AddressLine1 = addressLine1;
            Postcode = postcode;
            IsPrimary = isPrimary;
            CreatedDate = createdDate ?? DateTime.UtcNow;
        }

        public Guid Id { get; }
        public Guid ContactId { get; }
        public int ExpiresMonth { get; set; }
        public int ExpiresYear { get; set; }
        public string ObscuredCardNumber { set; get; }
        public string CardHolderFullName { set; get; }
        public string AddressLine1 { set; get; }
        public string Postcode { set; get; }
        public bool IsPrimary { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}