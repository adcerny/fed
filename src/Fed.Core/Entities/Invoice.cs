using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Entities
{
    public class Invoice
    {
        public Guid Id { get; set; }
        public Guid ContactId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string ExternalInvoiceNumber { get; set; }
        public string ExternalInvoiceId { get; set; }
        public string Reference { get; set; }
        public DateTime DateGenerated { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<Delivery> Deliveries { get; set; }
        public List<CardTransaction> Payments { get; set; }
        public List<CardTransaction> Refunds { get; set; }
        public List<DeliveryShortage> DeliveryShortages { get; set; }
        public List<DeliveryAddition> DeliveryAdditions { get; set; }
    }
}
