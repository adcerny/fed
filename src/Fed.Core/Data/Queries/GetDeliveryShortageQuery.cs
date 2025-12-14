using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Queries
{
    public class GetDeliveryShortageQuery : IDataOperation<DeliveryShortage>
    {
        public GetDeliveryShortageQuery(Guid orderId, string productId)
        {
            OrderId = orderId;
            ProductId = productId;
        }

        public Guid OrderId { get; }
        public string ProductId { get; }
    }
}