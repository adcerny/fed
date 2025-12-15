using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class HolidaysHandler : IHolidaysHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public HolidaysHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<IList<Holiday>> ExecuteAsync(GetHolidaysQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "SELECT * FROM [dbo].[Holidays] WHERE [Date] >= @FromDate AND [Date] <= @ToDate";

                var result = await connection.QueryAsync<Holiday>(
                    sql,
                    new { FromDate = query.DateRange.From.Value, ToDate = query.DateRange.To.Value });

                return result.ToList();
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<Holiday> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = await SqlQueryReader.GetSqlQueryAsync("InsertHoliday.sql");

                await connection.ExecuteAsync(sql, new { Date = cmd.Object.Date.Value, cmd.Object.Name });

                return true;
            }
        }

        public async Task<bool> ExecuteAsync(DeleteHolidayCommand cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "DELETE FROM [dbo].[Holidays] WHERE [Date] = @Date";

                await connection.ExecuteAsync(sql, new { Date = cmd.Date.Value });

                return true;
            }
        }
    }
}
