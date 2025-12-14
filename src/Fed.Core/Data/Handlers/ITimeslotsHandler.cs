using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface ITimeslotsHandler :
        IDataOperationHandler<GetTimeslotsQuery, IList<Timeslot>>,
        IDataOperationHandler<GetByIdQuery<Timeslot>, Timeslot>
    { }
}