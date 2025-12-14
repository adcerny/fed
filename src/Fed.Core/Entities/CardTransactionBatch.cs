using System;
using System.Collections.Generic;

namespace Fed.Core.Entities
{
    public class CardTransactionBatch
    {
        public CardTransactionBatch(
            Guid id,
            DateTime deliveryDate,
            DateTime timeStarted,
            DateTime? timeEnded,
            IList<CardTransaction> cardTransactions)
        {
            Id = id;
            DeliveryDate = deliveryDate;
            TimeStarted = timeStarted;
            TimeEnded = timeEnded;
            CardTransactions = cardTransactions;
        }

        public Guid Id { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime TimeStarted { get; set; }
        public DateTime? TimeEnded { get; set; }
        public IList<CardTransaction> CardTransactions { get; set; }
    }
}
