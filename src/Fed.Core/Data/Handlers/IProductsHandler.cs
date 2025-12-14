using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IProductsHandler :
        IDataOperationHandler<GetProductsQuery, IList<Product>>,
        IDataOperationHandler<GetByIdQuery<Product>, Product>,
        IDataOperationHandler<CreateCommand<Product>, bool>,
        IDataOperationHandler<UpdateCommand<Product>, bool>,
        IDataOperationHandler<DeleteCommand<string>, bool>
    { }
}