namespace Fed.Core.Exceptions
{
    public class OrderHasNoItemsException : FedException
    {
        public OrderHasNoItemsException(string orderName)
            : base(ErrorCode.OrderHasNoItems, $"The order with the name '{orderName}' cannot have zero items. Please cancel the order if you wish to remove the order.")
        { }
    }
}
