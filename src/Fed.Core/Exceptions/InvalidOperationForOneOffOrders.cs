namespace Fed.Core.Exceptions
{
    public class InvalidOperationForOneOffOrders : FedException
    {
        public InvalidOperationForOneOffOrders()
            : base(
                  ErrorCode.InvalidOperationForOneOffOrders,
                  "Cannot perform this operation for one off orders.")
        { }
    }
}
