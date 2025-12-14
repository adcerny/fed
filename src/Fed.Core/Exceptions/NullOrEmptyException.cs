namespace Fed.Core.Exceptions
{
    public class NullOrEmptyException : FedException
    {
        public NullOrEmptyException(string paramname)
            : base(
                  ErrorCode.NullOrEmptyName,
                  $"The value for '{paramname}' cannot be null or empty.")
        {
        }
    }
}
