using System;

namespace Fed.Core.Exceptions
{
    public class OrderNotFoundException : FedException
    {
        public OrderNotFoundException(Guid orderId)
            : base(
                  ErrorCode.OrderNotFound,
                  $"The order with ID {orderId} could not be found.")
        { }
    }
}
