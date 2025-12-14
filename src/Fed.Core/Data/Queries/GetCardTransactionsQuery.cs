using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetCardTransactionsQuery : IDataOperation<IList<CardTransaction>>
    {
        public GetCardTransactionsQuery(Guid? cardTransactionBatchId, CardTransactionStatus cardTransactionStatus)
        {
            CardTransactionBatchId = cardTransactionBatchId;
            CardTransactionStatus = cardTransactionStatus;
        }

        public Guid? CardTransactionBatchId { get; }

        public CardTransactionStatus? CardTransactionStatus { get; }

        public DateRange DeliveryDateRange { get; }
    }
}