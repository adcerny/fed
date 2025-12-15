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
    public class SuppliersHandler : ISuppliersHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public SuppliersHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<IList<Supplier>> ExecuteAsync(GetAllQuery<Supplier> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = "SELECT * FROM Suppliers";
                return (await connection.QueryAsync<Supplier>(sql)).ToList();

            }
        }

        public async Task<Supplier> ExecuteAsync(GetByIdQuery<Supplier> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = $"SELECT * FROM Suppliers WHERE Id = {query.Id}";
                return (await connection.QuerySingleAsync<Supplier>(sql));
            }
        }

        public async Task<Supplier> ExecuteAsync(CreateCommand<Supplier> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("InsertSupplier.sql");
                var obj = new
                {
                    cmd.Object.Id,
                    cmd.Object.Name
                };
                await connection.ExecuteAsync(sql, obj);

                return await ExecuteAsync(new GetByIdQuery<Supplier>(cmd.Object.Id.ToString()));
            }
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<Supplier> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("UpdateSupplier.sql");

                var obj = new
                {
                    cmd.Object.Id,
                    cmd.Object.Name
                };
                await connection.ExecuteAsync(sql, obj);
                return true;
            }
        }
    }
}
