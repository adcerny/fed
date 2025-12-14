using Fed.Core.ValueTypes;

namespace Fed.Core.Exceptions
{
    public class IvalidDateRangeException : FedException
    {
        public IvalidDateRangeException(Date from, Date to)
            : base(
                ErrorCode.IvalidDateRange,
                $"The values {from} - {to} is not a valid date range.")
        { }
    }
}
