using System;

namespace Fed.Core.Exceptions
{
    public class OrderDoesNotContainProductException : FedException
    {
        public OrderDoesNotContainProductException(Guid orderId, string productId)
            : base(
                  ErrorCode.OrderDoesNotContainProduct,
                  $"The order with ID {orderId} does not contain an order item with the product ID {productId}.")
        { }
    }
}
