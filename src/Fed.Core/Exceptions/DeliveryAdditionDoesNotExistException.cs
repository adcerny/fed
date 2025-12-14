using System;

namespace Fed.Core.Exceptions
{
    public class DeliveryAdditionDoesNotExistException : FedException
    {
        public DeliveryAdditionDoesNotExistException(Guid deliveryShortageId)
            : base(
                  ErrorCode.DeliveryAdditionDoesNotExist,
                  $"A delivery addition with ID {deliveryShortageId} does not exist.")
        { }
    }
}
