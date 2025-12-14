using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class DeliveryShortageHandler : IDeliveryShortageHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public DeliveryShortageHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig;
        }

        public async Task<IList<DeliveryShortage>> ExecuteAsync(GetDeliveryShortagesQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDeliveryShortages.sql");
                var obj = new { FromDate = query.DateRange.From.Value, ToDate = query.DateRange.To.Value };
                var result = await connection.QueryAsync<DeliveryShortage>(sql, obj);
                return result.ToList();
            }
        }

        public async Task<DeliveryShortage> ExecuteAsync(CreateCommand<DeliveryShortage> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var sql = await SqlQueryReader.GetSqlQueryAsync("InsertDeliveryShortage.sql");

                        await connection.ExecuteAsync(sql, cmd.Object, transaction);

                        transaction.Commit();

                        return cmd.Object;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<DeliveryShortage> ExecuteAsync(GetByIdQuery<DeliveryShortage> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDeliveryShortage.sql");
                var result = await connection.QuerySingleOrDefaultAsync<DeliveryShortage>(sql, query);
                return result;
            }
        }

        public async Task<DeliveryShortage> ExecuteAsync(GetDeliveryShortageQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("FindDeliveryShortage.sql");
                var result = await connection.QuerySingleOrDefaultAsync<DeliveryShortage>(sql, query);
                return result;
            }
        }

        public async Task<bool> ExecuteAsync(DeleteCommand<DeliveryShortage> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("DeleteDeliveryShortage.sql");
                await connection.ExecuteAsync(sql, new { cmd.Id });
                return true;
            }
        }
    }
}