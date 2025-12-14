using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IHolidaysHandler :
        IDataOperationHandler<GetHolidaysQuery, IList<Holiday>>,
        IDataOperationHandler<CreateCommand<Holiday>, bool>,
        IDataOperationHandler<DeleteHolidayCommand, bool>
    { }
}