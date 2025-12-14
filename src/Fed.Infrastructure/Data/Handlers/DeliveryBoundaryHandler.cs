using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class DeliveryBoundaryHandler : IDeliveryBoundaryHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public DeliveryBoundaryHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<IList<DeliveryBoundary>> ExecuteAsync(GetAllQuery<DeliveryBoundary> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = string.Concat(
                   "DECLARE @JSON NVARCHAR(MAX) = (",
                   " SELECT * ,",
                   "     JSON_QUERY((",
                   "         SELECT * FROM [dbo].[DeliveryBoundaryCoordinates] c",
                   "         WHERE c.DeliveryBoundaryId = d.Id",
                   "         ORDER BY c.SortIndex",
                   "         FOR JSON PATH",
                   "     )) AS MapCoordinates",
                   "     FROM[dbo].[DeliveryBoundaries] d",
                   " FOR JSON PATH",
                   " ) SELECT @JSON");

                return await connection.ReadJsonAsync<IList<DeliveryBoundary>>(sql);
            }
        }
    }
}