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
    public class CustomerMarketingAttributesHandler : ICustomerMarketingAttributesHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public CustomerMarketingAttributesHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }


        public async Task<IList<CustomerMarketingAttribute>> ExecuteAsync(GetAllQuery<CustomerMarketingAttribute> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                // string sql = await SqlQueryReader.GetSqlQueryAsync("GetCustomerMarketingAttributes.sql");
                const string sql = "SELECT * FROM [dbo].[CustomerMarketingAttributes]";
                var result = await connection.QueryAsync<CustomerMarketingAttribute>(sql, query);

                return result.ToList();
                //var result = await connection.QueryAsync<CustomerMarketingAttribute>(sql, query);
                //return result.ToList();
            }
        }

        public async Task<CustomerMarketingAttribute> ExecuteAsync(GetByIdQuery<CustomerMarketingAttribute> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                //string sql = await SqlQueryReader.GetSqlQueryAsync("GetCustomerMarketingAttributesById.sql");

                //return await connection.QueryFirstAsync<CustomerMarketingAttribute>(sql, new { query.Id });
                const string sql = "SELECT * FROM [dbo].[CustomerMarketingAttributes] where id=@id";
                var result = await connection.QueryFirstAsync<CustomerMarketingAttribute>(sql, new { query.Id });

                return result;
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<CustomerMarketingAttribute> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = await SqlQueryReader.GetSqlQueryAsync("InsertCustomerMarketingAttribute.sql");

                var result = await connection.ExecuteAsync(sql, cmd.Object);

                return true;
            }
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<CustomerMarketingAttribute> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("UpdateCustomerMarketingAttribute.sql");

                var obj = new
                {
                    cmd.Object.Id,
                    cmd.Object.Name,
                    cmd.Object.Description

                };
                await connection.ExecuteAsync(sql, obj);
                return true;
            }
        }
    }
}
