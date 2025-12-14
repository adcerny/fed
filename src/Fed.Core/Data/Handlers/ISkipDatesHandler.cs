using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface ISkipDatesHandler :
        IDataOperationHandler<GetSkipDatesQuery, IList<SkipDate>>,
        IDataOperationHandler<CreateSkipDateCommand, IList<SkipDate>>,
        IDataOperationHandler<DeleteSkipDateCommand, IList<SkipDate>>
    { }
}