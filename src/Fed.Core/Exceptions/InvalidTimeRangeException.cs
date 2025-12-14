namespace Fed.Core.Exceptions
{
    public class InvalidTimeRangeException : FedException
    {
        public InvalidTimeRangeException(int earliestHour, int latestHour)
            : base(
                ErrorCode.InvalidTimeRange,
                $"The values {earliestHour} - {latestHour} is not a valid time range.")
        { }
    }
}
