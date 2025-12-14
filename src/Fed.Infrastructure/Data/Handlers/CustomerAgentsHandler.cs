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
    public class CustomerAgentsHandler : ICustomerAgentsHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public CustomerAgentsHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }


        public async Task<IList<CustomerAgent>> ExecuteAsync(GetAllQuery<CustomerAgent> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "SELECT * FROM [dbo].[CustomerAgents]";

                var result = await connection.QueryAsync<CustomerAgent>(sql, query);

                return result.ToList();
            }
        }

        public async Task<CustomerAgent> ExecuteAsync(GetByIdQuery<CustomerAgent> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "SELECT * FROM [dbo].[CustomerAgents] WHERE Id = @Id";

                var result = await connection.QuerySingleAsync<CustomerAgent>(sql, query);

                return result;
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<CustomerAgent> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = await SqlQueryReader.GetSqlQueryAsync("InsertCustomerAgent.sql");

                var result = await connection.ExecuteAsync(sql, cmd.Object);

                return true;
            }
        }

        public async Task<bool> ExecuteAsync(DeleteCommand<CustomerAgent> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = await SqlQueryReader.GetSqlQueryAsync("DeleteCustomerAgent.sql");

                var result = await connection.ExecuteAsync(sql, cmd);

                return true;
            }
        }
    }
}
