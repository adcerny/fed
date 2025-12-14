namespace Fed.Core.Exceptions
{
    public class InvalidQuantityException : FedException
    {
        public InvalidQuantityException(int quantity)
            : base(
                  ErrorCode.InvalidQuantty,
                  $"The value '{quantity}' is not a valid quantity.")
        {
        }
    }
}
