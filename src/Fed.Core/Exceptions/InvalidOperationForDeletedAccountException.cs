namespace Fed.Core.Exceptions
{
    public class InvalidOperationForDeletedAccountException : FedException
    {
        public InvalidOperationForDeletedAccountException()
            : base(
                  ErrorCode.InvalidOperationForDeletedAccount,
                  "Deleted accounts cannot perform this operation.")
        { }
    }
}
