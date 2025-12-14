using Dapper;
using Fed.Core.Data.Handlers;
using Fed.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class ReportingHandler : IReportingHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public ReportingHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig;
        }

        public async Task<IList<T>> GetReportAsync<T>(string reportName, object queryArgs = null)
        {
            var sql = await SqlQueryReader.GetSqlQueryAsync(reportName);
            var result = await QueryReportAsync<T>(sql, queryArgs);
            return result;
        }

        public async Task<IList<T>> GetNewCustomerSummaryAsync<T>()
        {
            var sql = await SqlQueryReader.GetSqlQueryAsync("NewCustomerSummary");

            var today = DateTime.Now.ToBritishTime();
            var week1Number = today.MondayOfPreviousWeek().MondayOfPreviousWeek().MondayOfPreviousWeek().ToIsoWeekOfYear();
            var week2Number = today.MondayOfPreviousWeek().MondayOfPreviousWeek().ToIsoWeekOfYear();
            var week3Number = today.MondayOfPreviousWeek().ToIsoWeekOfYear();
            var week4Number = today.ToIsoWeekOfYear();

            var sqlQuery = sql
                .Replace("[Week-3]", $"[Week {week1Number}]")
                .Replace("[Week-2]", $"[Week {week2Number}]")
                .Replace("[Week-1]", $"[Week {week3Number}]")
                .Replace("[Week0]", $"[Week {week4Number}]");

            var result = await QueryReportAsync<T>(sqlQuery, null);
            return result;
        }

        private async Task<IList<T>> QueryReportAsync<T>(string sqlQuery, object queryArgs = null)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var result = await connection.QueryAsync<T>(sqlQuery, queryArgs);
                return result.ToList();
            }
        }
    }
}