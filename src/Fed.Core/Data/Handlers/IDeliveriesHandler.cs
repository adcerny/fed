using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IDeliveriesHandler :
        IDataOperationHandler<CreateCommand<IList<Delivery>>, IList<Delivery>>,
        IDataOperationHandler<GetDeliveriesQuery, IList<Delivery>>,
        IDataOperationHandler<GetByIdQuery<Delivery>, Delivery>,
        IDataOperationHandler<DeleteDeliveryCommand, bool>,
        IDataOperationHandler<SetDeliveryBagCountCommand, bool>,
        IDataOperationHandler<SetDeliveryPackingStatusCommand, bool>
    { }
}