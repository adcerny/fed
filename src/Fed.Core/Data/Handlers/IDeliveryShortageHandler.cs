using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IDeliveryShortageHandler :
        IDataOperationHandler<GetDeliveryShortagesQuery, IList<DeliveryShortage>>,
        IDataOperationHandler<GetDeliveryShortageQuery, DeliveryShortage>,
        IDataOperationHandler<GetByIdQuery<DeliveryShortage>, DeliveryShortage>,
        IDataOperationHandler<CreateCommand<DeliveryShortage>, DeliveryShortage>,
        IDataOperationHandler<DeleteCommand<DeliveryShortage>, bool>
    { }
}