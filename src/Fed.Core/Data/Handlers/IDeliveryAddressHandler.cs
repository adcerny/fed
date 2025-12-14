using Fed.Core.Data.Commands;
using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Handlers
{
    public interface IDeliveryAddressHandler :
        IDataOperationHandler<CreateDeliveryAddressCommand, Guid>,
        IDataOperationHandler<UpdateCommand<DeliveryAddress>, bool>,
        IDataOperationHandler<DeleteCommand<DeliveryAddress>, bool>
    { }
}