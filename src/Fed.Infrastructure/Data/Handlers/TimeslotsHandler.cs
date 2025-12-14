using Dapper;
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
    public class TimeslotsHandler : ITimeslotsHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public TimeslotsHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<IList<Timeslot>> ExecuteAsync(GetTimeslotsQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetTimeslotsByHub.sql");

                return (await connection.QueryAsync<Timeslot>(sql, query)).ToList();
            }
        }

        public async Task<Timeslot> ExecuteAsync(GetByIdQuery<Timeslot> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetTimeslotById.sql");

                return (await connection.QuerySingleAsync<Timeslot>(sql, query));
            }
        }
    }
}