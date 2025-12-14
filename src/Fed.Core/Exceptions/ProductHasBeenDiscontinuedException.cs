namespace Fed.Core.Exceptions
{
    public class ProductHasBeenDiscontinuedException : FedException
    {
        public ProductHasBeenDiscontinuedException(string productId)
            : base(
                  ErrorCode.ProductHasBeenDiscontinued,
                  $"The product with ID {productId} has been discontinued.")
        { }
    }
}
