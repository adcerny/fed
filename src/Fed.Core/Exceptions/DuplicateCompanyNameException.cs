namespace Fed.Core.Exceptions
{
    public class DuplicateCompanyNameException : FedException
    {
        public DuplicateCompanyNameException(string companyName)
            : base(
                  ErrorCode.DuplicateCompanyName,
                  $"The company name '{companyName}' is already taken.")
        { }
    }
}
