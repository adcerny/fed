namespace Fed.Core.Exceptions
{
    public class InvalidHourException : FedException
    {
        public InvalidHourException(int hour)
            : base(
                  ErrorCode.InvalidHour,
                  $"The value '{hour}' is not a valid hour. It has to be a value between 0 and 23.")
        { }
    }
}
