using Fed.Core.Data.Commands;
using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Handlers
{
    public interface IBillingAddressHandler :
        IDataOperationHandler<CreateBillingAddressCommand, Guid>,
        IDataOperationHandler<UpdateCommand<BillingAddress>, bool>,
        IDataOperationHandler<DeleteCommand<BillingAddress>, bool>
    { }
}