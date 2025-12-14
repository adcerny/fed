namespace Fed.Core.Exceptions
{
    public class MissingReasonForShortageException : FedException
    {
        public MissingReasonForShortageException()
            : base(
                  ErrorCode.MissingReasonForShortage,
                  "You must provide a reason for shorting an item.")
        { }
    }
}
