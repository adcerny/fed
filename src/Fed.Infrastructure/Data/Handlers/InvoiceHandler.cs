using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using Fed.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class InvoicesHandler : IInvoicesHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public InvoicesHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<Invoice> ExecuteAsync(GetByIdQuery<Invoice> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetInvoiceById.sql");

                return await connection.QuerySingleAsync<Invoice>(sql, new { query.Id });
            }
        }

        public async Task<IList<Invoice>> ExecuteAsync(DateRange query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetInvoicesByDate.sql");

                var obj = new { From = query.From.Value, To = query.To.Value };

                return await connection.ReadJsonAsync<IList<Invoice>>(sql, obj);
            }
        }

        public async Task<IList<Invoice>> ExecuteAsync(GetInvoicesQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetInvoicesByContactId.sql");

                return await connection.ReadJsonAsync<IList<Invoice>>(sql, query);
            }
        }

        public async Task<Guid> ExecuteAsync(CreateCommand<Invoice> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var invoiceId = Guid.NewGuid();

                var sql = await SqlQueryReader.GetSqlQueryAsync("InsertInvoice.sql");

                var obj =
                    new
                    {
                        Id = invoiceId,
                        cmd.Object.ContactId,
                        cmd.Object.FromDate,
                        cmd.Object.ToDate,
                        cmd.Object.ExternalInvoiceNumber,
                        cmd.Object.ExternalInvoiceId,
                        cmd.Object.DateGenerated
                    };

                await connection.ExecuteAsync(sql, obj);

                var orderSql = string.Concat(
                    "INSERT INTO dbo.InvoiceDeliveries ",
                    "( InvoiceId, DeliveryId ) ",
                    " VALUES ",
                    string.Join($", ", cmd.Object.Deliveries.Select(o => $"('{invoiceId}',  '{o.Id}')")));

                await connection.ExecuteAsync(orderSql);

                return invoiceId;
            }
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<Invoice> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat(
                    "UPDATE [dbo].[Invoices] ",
                    "   SET [ContactId] = @ContactId ",
                    "      ,[FromDate] = @FromDate ",
                    "      ,[ToDate] = @ToDate ",
                    "      ,[ExternalInvoiceNumber] = @ExternalInvoiceNumber, ",
                    "      ,[ExternalInvoiceId] = @ExternalInvoiceId, ",
                    "      ,[DateGenerated] = @DateGenerated ",
                    " WHERE Id = @Id");

                var obj =
                    new
                    {
                        cmd.Id,
                        cmd.Object.ContactId,
                        cmd.Object.FromDate,
                        cmd.Object.ToDate,
                        cmd.Object.ExternalInvoiceNumber,
                        cmd.Object.ExternalInvoiceId,
                        cmd.Object.DateGenerated
                    };

                await connection.ExecuteAsync(sql, obj);

                return true;
            }
        }

        public async Task<Guid> ExecuteAsync(CreateCommand<CardTransactionBatch> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat(
                    "INSERT INTO [payment].[CardTransactionBatches] ",
                    "  ([Id] ",
                    "  ,[DeliveryDate] ",
                    "  ,[TimeStarted] ",
                    "  ,[TimeEnded])",
                    " VALUES ",
                    "  (@Id ",
                    "  ,@DeliveryDate ",
                    "  ,@TimeStarted ",
                    "  ,@TimeEnded )");

                var obj = new { cmd.Object.Id, cmd.Object.DeliveryDate, cmd.Object.TimeStarted, cmd.Object.TimeEnded };
                await connection.ExecuteAsync(sql, obj);

                return cmd.Object.Id;
            }
        }
    }
}
