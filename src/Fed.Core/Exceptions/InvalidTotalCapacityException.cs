namespace Fed.Core.Exceptions
{
    public class InvalidTotalCapacityException : FedException
    {
        public InvalidTotalCapacityException(decimal totalCapacity)
            : base(
                ErrorCode.InvalidTotalCapacity,
                $"The value '{totalCapacity}' is not a valid capacity.")
        { }
    }
}
