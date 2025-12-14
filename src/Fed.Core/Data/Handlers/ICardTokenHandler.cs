using Fed.Core.Data.Commands;
using Fed.Core.Entities;
using System;

namespace Fed.Core.Data.Handlers
{
    public interface ICardTokenHandler :
        IDataOperationHandler<CreateCardTokenCommand, Guid>,
        IDataOperationHandler<UpdateCommand<CardToken>, bool>,
        IDataOperationHandler<DeleteCommand<CardToken>, bool>
    { }
}