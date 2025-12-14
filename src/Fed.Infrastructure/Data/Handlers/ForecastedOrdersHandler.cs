using Dapper;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class ForecastedOrdersHandler : IForecastedOrdersHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public ForecastedOrdersHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<IList<ForecastedOrder>> ExecuteAsync(GetRecurringOrdersQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetForecastedOrders");
                var obj =
                    new
                    {
                        FromDate = query.DateRange.From.Value,
                        ToDate = query.DateRange.To.Value,
                        query.IncludeExpired,
                        query.IncludeFromDemoAccounts,
                        query.IncludeFromCancelledAccounts,
                        query.IncludeFromDeletedAccounts,
                        query.IncludeFromPausedAccounts,
                        query.ContactId
                    };

                var result = await connection.QueryAsync<ForecastedOrder>(sql, obj);
                return result.ToList();
            }
        }
    }
}
