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
    public class DeliveryAdditionHandler : IDeliveryAdditionHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public DeliveryAdditionHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig;
        }

        public async Task<DeliveryAddition> ExecuteAsync(GetDeliveryAdditionQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("FindDeliveryAddition.sql");
                var result = await connection.QuerySingleOrDefaultAsync<DeliveryAddition>(sql, query);
                return result;
            }
        }

        public async Task<DeliveryAddition> ExecuteAsync(CreateCommand<DeliveryAddition> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var sql = await SqlQueryReader.GetSqlQueryAsync("InsertDeliveryAddition.sql");

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

        public async Task<bool> ExecuteAsync(DeleteCommand<DeliveryAddition> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("DeleteDeliveryAddition.sql");
                await connection.ExecuteAsync(sql, new { cmd.Id });
                return true;
            }
        }

        public async Task<DeliveryAddition> ExecuteAsync(GetByIdQuery<DeliveryAddition> obj)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var query = await SqlQueryReader.GetSqlQueryAsync("GetDeliveryAdditionById.sql");
                var result = await connection.QuerySingleOrDefaultAsync<DeliveryAddition>(query, obj);
                return result;
            }
        }

        public async Task<IList<DeliveryAddition>> ExecuteAsync(GetDeliveryAdditionsQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDeliveryAdditions.sql");
                var obj = new { FromDate = query.DateRange.From.Value, ToDate = query.DateRange.To.Value };
                var result = await connection.QueryAsync<DeliveryAddition>(sql, obj);
                return result.ToList();
            }
        }
    }
}