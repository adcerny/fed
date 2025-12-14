using Fed.Core.ValueTypes;
using System;

namespace Fed.Core.Exceptions
{
    public class DateOutOfRangeException : FedException
    {
        public DateOutOfRangeException(DateTime dateTime)
            : base(
                  ErrorCode.DateOutOfRange,
                  $"The date '{dateTime.ToString("yyyy-MM-dd")}' must be greater than {Date.MinDate} and smaller than {Date.MaxDate}.")
        {
        }
    }
}