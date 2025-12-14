using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Queries
{
    public class GetDeliveryAdditionQuery : IDataOperation<DeliveryAddition>
    {
        public GetDeliveryAdditionQuery(Guid orderId, string productId)
        {
            OrderId = orderId;
            ProductId = productId;
        }

        public Guid OrderId { get; }
        public string ProductId { get; }
    }
}