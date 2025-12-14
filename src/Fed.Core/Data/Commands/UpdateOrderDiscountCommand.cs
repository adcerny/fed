using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Commands
{
    public class UpdateOrderDiscountCommand : IDataOperation<OrderItem>
    {
        public UpdateOrderDiscountCommand(Guid orderId, Guid discountId, Guid discountedProductsOrderId)
        {
            OrderId = orderId;
            DiscountId = discountId;
            DiscountedProductsOrderId = discountedProductsOrderId;
        }

        public Guid OrderId { get; }
        public Guid DiscountId { get; }
        public Guid DiscountedProductsOrderId { get; }
    }
}