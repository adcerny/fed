using System;
using System.Collections.Generic;

namespace Fed.Core.Entities
{
    public class DeliveryShortage
    {
        public DeliveryShortage(
            Guid id,
            Guid orderId,
            string productId,
            int desiredQuantity,
            int actualQuantity,
            string reason,
            string reasonCode,
            TimeSpan timeRecorded,
            decimal productPrice,
            string productName = null)
        {
            Id = id;
            OrderId = orderId;
            ProductId = productId;
            ProductName = productName;
            ProductPrice = productPrice;
            DesiredQuantity = desiredQuantity;
            ActualQuantity = actualQuantity;
            Reason = reason;
            TimeRecorded = timeRecorded;
            ReasonCode = reasonCode;
        }

        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; }
        public decimal ProductPrice { get; set; }
        public int DesiredQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public string Reason { get; set; }
        public string ReasonCode { get; set; }
        public TimeSpan TimeRecorded { get; set; }
        public IList<DeliveryAddition> Replacements { get; set; }
    }
}