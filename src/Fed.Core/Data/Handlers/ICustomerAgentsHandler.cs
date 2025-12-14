using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface ICustomerAgentsHandler :
        IDataOperationHandler<GetByIdQuery<CustomerAgent>, CustomerAgent>,
        IDataOperationHandler<GetAllQuery<CustomerAgent>, IList<CustomerAgent>>,
        IDataOperationHandler<CreateCommand<CustomerAgent>, bool>,
        IDataOperationHandler<DeleteCommand<CustomerAgent>, bool>
    { }
}