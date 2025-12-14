using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IRecurringOrdersHandler :
        IDataOperationHandler<GetRecurringOrdersQuery, IList<RecurringOrder>>,
        IDataOperationHandler<GetByIdQuery<RecurringOrder>, RecurringOrder>,
        IDataOperationHandler<GetByIdsQuery<RecurringOrder>, IList<RecurringOrder>>,
        IDataOperationHandler<CreateCommand<RecurringOrder>, RecurringOrder>,
        IDataOperationHandler<UpdateCommand<RecurringOrder>, RecurringOrder>,
        IDataOperationHandler<DeleteCommand<RecurringOrder>, bool>
    {
    }
}