using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System;

namespace Fed.Core.Data.Queries
{
    public class GetPaymentRequestsQuery : IDataOperation<Order>
    {
        public GetPaymentRequestsQuery(Date deliveryDate, Guid batchId)
        {
            DeliveryDate = deliveryDate;
            CardTransactionBatchId = batchId;
        }

        public Date DeliveryDate { get; }
        public Guid CardTransactionBatchId { get; }
    }
}