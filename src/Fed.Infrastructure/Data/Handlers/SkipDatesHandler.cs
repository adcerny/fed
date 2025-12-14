using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class SkipDatesHandler : ISkipDatesHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public SkipDatesHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        private static async Task<IList<SkipDate>> GetSkipDatesAsync(SqlConnection connection, GetSkipDatesQuery query)
        {
            var sql = await SqlQueryReader.GetSqlQueryAsync("GetSkipDates.sql");

            var result = await connection.QueryAsync<SkipDate>(sql, new { query.RecurringOrderId, From = query.DateRange?.From.Value, To = query.DateRange?.To.Value });

            return result.ToList();
        }

        public async Task<IList<SkipDate>> ExecuteAsync(GetSkipDatesQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                return await GetSkipDatesAsync(connection, query);
            }
        }

        public async Task<IList<SkipDate>> ExecuteAsync(CreateSkipDateCommand cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat("IF NOT EXISTS (SELECT * FROM SkipDates WHERE RecurringOrderId = @RecurringOrderId AND Date = @Date) ",
                                          "INSERT INTO [dbo].[SkipDates] ([RecurringOrderId], [Date], [Reason], [CreatedBy], [CreatedDateTime]) " +
                                          "VALUES (@RecurringOrderId, @Date, @Reason, @CreatedBy, @CreatedDateTime)");

                await connection.ExecuteAsync(sql, new { cmd.RecurringOrderId, Date = cmd.Date.Value, cmd.Reason, cmd.CreatedBy, cmd.CreatedDateTime });

                var query = new GetSkipDatesQuery(cmd.RecurringOrderId, null);
                return await GetSkipDatesAsync(connection, query);
            }
        }

        public async Task<IList<SkipDate>> ExecuteAsync(DeleteSkipDateCommand cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "DELETE FROM [dbo].[SkipDates] WHERE [RecurringOrderId] = @RecurringOrderId AND [Date] = @Date";

                await connection.ExecuteAsync(sql, new { cmd.RecurringOrderId, Date = cmd.Date.Value });

                var query = new GetSkipDatesQuery(cmd.RecurringOrderId, null);
                return await GetSkipDatesAsync(connection, query);
            }
        }
    }
}