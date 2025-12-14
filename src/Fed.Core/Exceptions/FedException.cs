using System;

namespace Fed.Core.Exceptions
{
    public class FedException : Exception
    {
        public readonly ErrorCode ErrorCode;

        public FedException(ErrorCode code, string message)
            : base(message)
        {
            ErrorCode = code;
        }
    }
}
