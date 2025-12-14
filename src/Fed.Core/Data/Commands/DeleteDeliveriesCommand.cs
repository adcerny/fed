using Fed.Core.ValueTypes;

namespace Fed.Core.Data.Commands
{
    public class DeleteDeliveryCommand : IDataOperation<bool>
    {
        public DeleteDeliveryCommand(Date deliveryDate)
        {
            DeliveryDate = deliveryDate;
        }
        public Date DeliveryDate { get; }
    }
}