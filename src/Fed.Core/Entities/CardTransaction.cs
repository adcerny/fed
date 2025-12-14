using Fed.Core.Enums;
using System;

namespace Fed.Core.Entities
{
    public class CardTransaction
    {
        public CardTransaction(
            Guid id,
            Guid batchId,
            Guid deliveryId,
            Guid cardTokenId,
            CardTransactionStatus status,
            DateTime timeCreated,
            DateTime? timeModified,
            decimal amountRequested,
            decimal? amountCaptured,
            string errorMessage,
            string responseCode,
            string responseText)
        {
            Id = id;
            CardTransactionBatchId = batchId;
            CardTokenId = cardTokenId;
            DeliveryId = deliveryId;
            Status = status;
            TimeCreated = timeCreated;
            TimeModified = timeModified;
            AmountRequested = amountRequested;
            AmountCaptured = amountCaptured;
            ErrorMessage = errorMessage;
            ResponseCode = responseCode;
            ResponseText = responseText;
        }

        public Guid Id { get; set; }
        public Guid CardTransactionBatchId { get; set; }
        public Guid CardTokenId { get; set; }
        public Guid DeliveryId { get; set; }
        public CardTransactionStatus Status { get; set; }
        public DateTime TimeCreated { get; }
        public DateTime? TimeModified { get; set; }
        public decimal AmountRequested { get; set; }
        public decimal? AmountCaptured { get; set; }
        public string ErrorMessage { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseText { get; set; }
    }
}
