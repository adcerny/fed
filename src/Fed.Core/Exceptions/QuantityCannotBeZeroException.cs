namespace Fed.Core.Exceptions
{
    public class QuantityCannotBeZeroException : FedException
    {
        public QuantityCannotBeZeroException() : base(ErrorCode.QuantityCannotBeZero, "Quantity cannot be zero.") { }
    }
}
