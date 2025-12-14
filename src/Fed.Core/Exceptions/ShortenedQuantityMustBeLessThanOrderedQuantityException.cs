namespace Fed.Core.Exceptions
{
    public class ShortenedQuantityMustBeLessThanOrderedQuantityException : FedException
    {
        public ShortenedQuantityMustBeLessThanOrderedQuantityException(int desiredQuantity, int actualQuantity)
            : base(
                  ErrorCode.ShortenedQuantityMustBeLessThanOrderedQuantity,
                  $"Cannot short the selected order item, because the actual quantity of {actualQuantity} is greater or equal to the desired quantity {desiredQuantity}.")
        { }
    }
}