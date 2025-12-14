using Fed.Core.Enums;
using System;

namespace Fed.Core.Entities
{
    public class DeliveryAddition
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid DeliveryShortageId { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductCode { get; set; }
        public decimal ProductPrice { get; set; }
        public bool IsTaxable { get; set; }
        public int Quantity { get; set; }
        public DeliveryAdditionReason Reason { get; set; }
        public string Notes { get; set; }
        public TimeSpan TimeRecorded { get; set; }

        public DeliveryAddition(
            Guid id,
            Guid orderId,
            string productId,
            int quantity,
            int reason,
            string notes,
            TimeSpan timeRecorded,
            Guid deliveryShortageId,
            string productName,
            string productCode,
            decimal productPrice,
            bool isTaxable
            )
        {
            Id = id;
            OrderId = orderId;
            ProductId = productId;
            Quantity = quantity;
            Reason = (DeliveryAdditionReason)reason;
            Notes = notes;
            TimeRecorded = timeRecorded;
            ProductName = productName;
            ProductCode = productCode;
            ProductPrice = productPrice;
            IsTaxable = isTaxable;
            DeliveryShortageId = deliveryShortageId;
        }

        public static DeliveryAddition CreateSubstitute(
            Guid orderId,
            string productId,
            string productName,
            string productCode,
            decimal productPrice,
            bool isTaxable,
            int quantity,
            string notes,
            Guid deliveryShortageId)
            => new DeliveryAddition(
                Guid.NewGuid(),
                orderId,
                productId,
                quantity,
                (int)DeliveryAdditionReason.Substitute,
                notes,
                DateTime.Now.TimeOfDay,
                deliveryShortageId,
                productName,
                productCode,
                productPrice,
                isTaxable
                );
    }
}