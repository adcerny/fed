using System;

namespace Fed.Core.Exceptions
{
    public class ProductAlreadyShortenedException : FedException
    {
        public ProductAlreadyShortenedException(Guid orderId, string productId)
            : base(
                  ErrorCode.ProductAlreadyShortened,
                  $"The product with ID {productId} has already been shortened for the order with ID {orderId}. If you wish to overwrite the existing shortage then please delete the record first and create a new shortage afterwards.")
        { }
    }
}
