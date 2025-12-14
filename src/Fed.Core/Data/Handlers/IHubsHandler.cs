using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IHubsHandler :
        IDataOperationHandler<GetHubsQuery, IList<Hub>>,
        IDataOperationHandler<UpdateCommand<Hub>, Hub>
    {
    }
}