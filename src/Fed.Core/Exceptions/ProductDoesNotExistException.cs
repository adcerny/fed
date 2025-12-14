namespace Fed.Core.Exceptions
{
    public class ProductDoesNotExistException : FedException
    {
        public ProductDoesNotExistException(string productId)
            : base(
                  ErrorCode.ProductDoesNotExist,
                  $"The product with ID {productId} does not exist.")
        { }
    }
}
