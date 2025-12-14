using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface ISuppliersHandler :
        IDataOperationHandler<GetAllQuery<Supplier>, IList<Supplier>>,
        IDataOperationHandler<GetByIdQuery<Supplier>, Supplier>,
        IDataOperationHandler<CreateCommand<Supplier>, Supplier>,
        IDataOperationHandler<UpdateCommand<Supplier>, bool>
    {
    }
}