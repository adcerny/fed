namespace Fed.Core.Exceptions
{
    public class InvalidWeeklyRecurrenceException : FedException
    {
        public InvalidWeeklyRecurrenceException(int weeklyRecurrence)
            : base(
                  ErrorCode.InvalidWeeklyRecurrence,
                  $"The value '{weeklyRecurrence}' is not a valid weekly recurrence.")
        {
        }
    }
}
