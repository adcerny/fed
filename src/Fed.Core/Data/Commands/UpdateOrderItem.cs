using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Commands
{
    public class UpdateOrderItemCommand : IDataOperation<OrderItem>
    {
        public UpdateOrderItemCommand(Guid orderId, string productId, OrderItem orderItem)
        {
            OrderId = orderId;
            ProductId = productId;
            OrderItem = orderItem;
        }

        public Guid OrderId { get; }
        public string ProductId { get; }
        public OrderItem OrderItem { get; }
    }
}