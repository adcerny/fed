using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IDeliveryAdditionHandler :
        IDataOperationHandler<GetDeliveryAdditionQuery, DeliveryAddition>,
        IDataOperationHandler<GetByIdQuery<DeliveryAddition>, DeliveryAddition>,
        IDataOperationHandler<CreateCommand<DeliveryAddition>, DeliveryAddition>,
        IDataOperationHandler<DeleteCommand<DeliveryAddition>, bool>,
        IDataOperationHandler<GetDeliveryAdditionsQuery, IList<DeliveryAddition>>
    { }
}