using System;

namespace Fed.Core.Exceptions
{
    public class DeliveryShortageDoesNotExistException : FedException
    {
        public DeliveryShortageDoesNotExistException(Guid deliveryShortageId)
            : base(
                  ErrorCode.DeliveryShortageDoesNotExist,
                  $"A delivery shortage with ID {deliveryShortageId} does not exist.")
        { }
    }
}
