using System;

namespace Fed.Core.Enums
{
    public struct PaymentRequest
    {
        public Guid CardTransactionBatchId { get; set; }
        public Guid DeliveryId { get; set; }
        public Guid CardTokenId { get; set; }
        public decimal Amount { get; set; }
        public Guid? OriginalCardTransactionId { get; set; }
        public CardTransactionStatus Status { get; set; }
    }
}
