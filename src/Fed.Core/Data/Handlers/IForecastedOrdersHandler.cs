using Fed.Core.Data.Queries;
using Fed.Core.Models;
using System.Collections.Generic;

namespace Fed.Core.Data.Handlers
{
    public interface IForecastedOrdersHandler :
        IDataOperationHandler<GetRecurringOrdersQuery, IList<ForecastedOrder>>
    {
    }
}