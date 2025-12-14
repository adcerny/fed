using Fed.Core.Extensions;

namespace Fed.Core.Exceptions
{
    public class PastCutOffException : FedException
    {
        public PastCutOffException()
            : base(
                  ErrorCode.PastCutOff,
                  $"One off orders for the next day cannot be updated after the cut off time of {DateTimeExtensions.DailyCutOffTime}.")
        {
        }
    }
}
