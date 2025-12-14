using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class PostcodeLocationHandler : IPostcodeLocationHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public PostcodeLocationHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<PostcodeLocation> ExecuteAsync(GetPostcodeLocationQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = $"SELECT * FROM [dbo].[PostcodeLocations] WHERE POSTCODE = @Postcode";

                return (await connection.QueryAsync<PostcodeLocation>(sql, new { query.Postcode })).FirstOrDefault();
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<PostcodeLocation> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var obj = new { Id = Guid.NewGuid(), cmd.Object.Postcode, cmd.Object.Coordinate.Latitude, cmd.Object.Coordinate.Longitude };

                var sql = $"IF NOT EXISTS (SELECT * FROM [dbo].[PostcodeLocations] WHERE [Postcode] = @Postcode) INSERT INTO [dbo].[PostcodeLocations] ([Id], [Postcode], [Latitude], [Longitude]) VALUES(@Id, @Postcode, @Latitude, @Longitude)";

                await connection.ExecuteAsync(sql, obj);

                return true;
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<PostcodeQuery> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var obj = new { Id = Guid.NewGuid(), cmd.Object.Postcode, QueryDate = DateTime.UtcNow, cmd.Object.Deliverable };

                var sql = $"INSERT INTO [dbo].[PostcodeQueries] ([Id], [Postcode], [QueryDate], [Deliverable]) VALUES (@Id, @Postcode, @QueryDate, @Deliverable)";

                await connection.ExecuteAsync(sql, obj);

                return true;
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<PostcodeContact> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var obj = new { Id = Guid.NewGuid(), cmd.Object.Postcode, cmd.Object.Email, DateAdded = DateTime.UtcNow, cmd.Object.IsDeliverable };

                var sql = $"INSERT INTO [dbo].[PostcodeContacts] ([Id], [Postcode], [Email], [DateAdded], [IsDeliverable]) VALUES (@Id, @Postcode, @Email, @DateAdded, @IsDeliverable)";

                await connection.ExecuteAsync(sql, obj);

                return true;
            }
        }
    }
}