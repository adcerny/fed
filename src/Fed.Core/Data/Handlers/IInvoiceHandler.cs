using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IInvoicesHandler :
        IDataOperationHandler<GetByIdQuery<Invoice>, Invoice>,
        IDataOperationHandler<GetInvoicesQuery, IList<Invoice>>,
        IDataOperationHandler<CreateCommand<Invoice>, Guid>,
        IDataOperationHandler<UpdateCommand<Invoice>, bool>,
        IDataOperationHandler<DateRange, IList<Invoice>>
    {
    }
}
