using System;

namespace Fed.Core.Exceptions
{
    public class ProductSubstituteAlreadyAddedException : FedException
    {
        public ProductSubstituteAlreadyAddedException(Guid orderId, string productId)
            : base(
                  ErrorCode.ProductSubstituteAlreadyAdded,
                  $"The product with ID {productId} has already been added as a substitute for the order with ID {orderId}. If you wish to overwrite the existing substitute then please delete the old substitute first and create a new substitute afterwards.")
        { }
    }
}
