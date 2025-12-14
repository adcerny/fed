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
    public class HubsHandler : IHubsHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public HubsHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<IList<Hub>> ExecuteAsync(GetHubsQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "SELECT * FROM [dbo].[Hubs]";

                return (await connection.QueryAsync<Hub>(sql, query)).ToList();
            }
        }

        public async Task<Hub> ExecuteAsync(UpdateCommand<Hub> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "UPDATE [dbo].[Hubs] SET [Name] = @Name, [Postcode] = @Postcode, [AddressLine1] = @AddressLine1, [AddressLine2] = @AddressLine2, [OrderDeadline] = @OrderDeadline WHERE [Id] = @Id";

                var obj = new { cmd.Id, cmd.Object.Name, cmd.Object.Postcode, cmd.Object.AddressLine1, cmd.Object.AddressLine2, cmd.Object.OrderDeadline };
                await connection.ExecuteAsync(sql, obj);

                var hubs = await
                    connection.QueryAsync<Hub>(
                        "SELECT * FROM [dbo].[Hubs] WHERE [Id] = @Id",
                        new { cmd.Id });

                return hubs.SingleOrDefault();
            }
        }
    }
}