namespace Fed.Core.Data.Commands
{
    public class SetDeliveryBagCountCommand : IDataOperation<bool>
    {
        public SetDeliveryBagCountCommand(string deliveryId, int bagCount)
        {
            DeliveryId = deliveryId;
            BagCount = bagCount;
        }

        public string DeliveryId { get; }
        public int BagCount { get; }
    }
}