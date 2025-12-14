using Fed.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fed.Core.Entities
{
    public class Contact
    {
        private readonly bool _isDeleted;

        public Contact(
            Guid id,
            string shortId,
            Guid customerId,
            string title,
            string firstName,
            string lastName,
            string email,
            string phone,
            bool isMarketingConsented,
            bool isDeleted,
            int paymentMethodId,
            DateTime? createdDate = null,
            IList<DeliveryAddress> deliveryAddresses = null,
            IList<BillingAddress> billingAddresses = null,
            IList<CardToken> cardTokens = null)
        {
            Id = id;
            ShortId = shortId;
            CustomerId = customerId;
            Title = title;
            FirstName = firstName;
            LastName = lastName;
            Email = email?.Trim();
            Phone = phone;
            IsMarketingConsented = isMarketingConsented;
            _isDeleted = isDeleted;
            PaymentMethod = (PaymentMethod)paymentMethodId;
            DeliveryAddresses = deliveryAddresses;
            BillingAddresses = billingAddresses;
            CardTokens = cardTokens;
            CreatedDate = createdDate ?? DateTime.UtcNow;
        }

        public Guid Id { get; }
        public string ShortId { get; }
        public Guid CustomerId { get; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool IsMarketingConsented { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public IList<DeliveryAddress> DeliveryAddresses { get; set; }
        public IList<BillingAddress> BillingAddresses { get; set; }
        public IList<CardToken> CardTokens { get; }
        public DateTime CreatedDate { get; set; }

        public DeliveryAddress GetPrimaryDeliveryAddress() =>
            DeliveryAddresses?.SingleOrDefault(a => a.IsPrimary);

        public BillingAddress GetPrimaryBillingAddress() =>
            BillingAddresses?.SingleOrDefault(a => a.IsPrimary);

        public static Contact CreateEmpty() =>
            new Contact(
                Guid.Empty,     // Contact ID
                string.Empty,   // Contact Short ID
                Guid.Empty,     // Customer ID
                null,           // Title
                "",
                "",
                "",
                "",
                true,
                false,          // Is Deleted
                (int)PaymentMethod.Card,
                deliveryAddresses: new List<DeliveryAddress>(),
                billingAddresses: new List<BillingAddress>());
    }
}