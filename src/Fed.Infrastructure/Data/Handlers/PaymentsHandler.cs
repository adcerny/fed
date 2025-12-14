using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class PaymentRequestsHandler : IPaymentsHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public PaymentRequestsHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<IList<PaymentRequest>> ExecuteAsync(GetPaymentRequestsQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetPaymentRequests.sql");

                return (await connection.QueryAsync<PaymentRequest>(sql, new { query.CardTransactionBatchId, DeliveryDate = query.DeliveryDate.Value, Status = CardTransactionStatus.Paid })).ToList();
            }
        }

        public async Task<IList<Guid>> ExecuteAsync(GetDeliveryIdsForPaymentQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDeliveryIdsForPayment.sql");

                return (await connection.QueryAsync<Guid>(sql, new { DeliveryDate = query.DeliveryDate.Value })).ToList();
            }
        }

        public async Task<IList<(Guid, Guid)>> ExecuteAsync(GetDeliveryAndTrasactionIdsForRefundQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDeliveryAndTrasactionIdsForRefund.sql");

                return (await connection.QueryAsync<(Guid, Guid)>(sql, new { DeliveryDate = query.DeliveryDate.Value })).ToList();
            }
        }

        public async Task<Guid> ExecuteAsync(CreateCommand<CardTransaction> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat(
                    "INSERT INTO [dbo].[CardTransactions] ",
                    "  ([Id] ",
                    "  ,[CardTransactionBatchId] ",
                    "  ,[DeliveryId] ",
                    "  ,[CardTokenId] ",
                    "  ,[CardTransactionStatusId] ",
                    "  ,[TimeCreated] ",
                    "  ,[TimeModified] ",
                    "  ,[AmountRequested] ",
                    "  ,[AmountCaptured] ",
                    "  ,[ErrorMessage] ",
                    "  ,[ResponseCode] ",
                    "  ,[ResponseText])",
                    " VALUES ",
                    "  (@Id ",
                    "  ,@CardTransactionBatchId ",
                    "  ,@DeliveryId ",
                    "  ,@CardTokenId ",
                    "  ,@Status ",
                    "  ,@TimeCreated ",
                    "  ,@TimeModified ",
                    "  ,@AmountRequested ",
                    "  ,@AmountCaptured ",
                    "  ,@ErrorMessage ",
                    "  ,@ResponseCode ",
                    "  ,@ResponseText)");

                var obj = new { cmd.Object.Id, cmd.Object.CardTransactionBatchId, cmd.Object.DeliveryId, cmd.Object.CardTokenId, cmd.Object.Status, cmd.Object.TimeCreated, cmd.Object.TimeModified, cmd.Object.AmountRequested, cmd.Object.AmountCaptured, cmd.Object.ErrorMessage, cmd.Object.ResponseCode, cmd.Object.ResponseText };
                await connection.ExecuteAsync(sql, obj);

                return cmd.Object.Id;
            }
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<CardTransaction> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat(
                    "UPDATE [dbo].[CardTransactions] ",
                    "SET [Id] = @Id, ",
                    "[CardTransactionBatchId] = @CardTransactionBatchId, ",
                    "[DeliveryId] = @DeliveryId, ",
                    "[CardTokenId] = @CardTokenId, ",
                    "[CardTransactionStatusId] = @Status, ",
                    "[TimeCreated] = @TimeCreated, ",
                    "[TimeModified] = @TimeModified, ",
                    "[AmountRequested] = @AmountRequested, ",
                    "[AmountCaptured] = @AmountCaptured, ",
                    "[ErrorMessage] = @ErrorMessage, ",
                    "[ResponseCode] = @ResponseCode, ",
                    "[ResponseText] = @ResponseText ",
                    "WHERE [Id] = @Id");

                var obj = new { cmd.Id, cmd.Object.CardTransactionBatchId, cmd.Object.DeliveryId, cmd.Object.CardTokenId, cmd.Object.Status, cmd.Object.TimeCreated, cmd.Object.TimeModified, cmd.Object.AmountRequested, cmd.Object.AmountCaptured, cmd.Object.ErrorMessage, cmd.Object.ResponseCode, cmd.Object.ResponseText };
                await connection.ExecuteAsync(sql, obj);

                return true;
            }
        }

        public async Task<Guid> ExecuteAsync(CreateCommand<CardTransactionBatch> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat(
                    "INSERT INTO [dbo].[CardTransactionBatches] ",
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

        public async Task<bool> ExecuteAsync(UpdateCommand<CardTransactionBatch> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat(
                    "UPDATE [dbo].[CardTransactionBatches] ",
                    "SET [Id] = @Id ",
                    ",[DeliveryDate] = @DeliveryDate ",
                    ",[TimeStarted] = @TimeStarted  ",
                    ",[TimeEnded] = @TimeEnded ",
                    "WHERE [Id] = @Id");

                var obj = new { cmd.Id, cmd.Object.DeliveryDate, cmd.Object.TimeStarted, cmd.Object.TimeEnded };
                await connection.ExecuteAsync(sql, obj);

                return true;
            }
        }

        public async Task<IList<CardTransaction>> ExecuteAsync(GetCardTransactionsQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat(
                    " SELECT * FROM [dbo].[CardTransactions] " +
                    " WHERE [CardTransactionBatchId] = @CardTransactionBatchId Or @CardTransactionBatchId IS NULL ",
                    " AND [CardTransactionStatusId] =  @CardTransactionStatus Or @CardTransactionStatus IS NULL "
                );
                return (await connection.QueryAsync<CardTransaction>(sql, new { query.CardTransactionBatchId, query.CardTransactionStatus })).ToList();
            }
        }


        public async Task<CardTransactionBatch> ExecuteAsync(GetByIdQuery<CardTransactionBatch> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat(
                    "DECLARE @JSON NVARCHAR(MAX) =										 ",
                    "	(SELECT *, JSON_QUERY((SELECT *,                                 ",
                    "                          CardTransactionStatusId AS Status   	     ",
                    "                        FROM [dbo].[CardTransactions] [t]			 ",
                    "                        WHERE [t].[CardTransactionBatchId]=[b].[Id] ",
                    "                        FOR JSON PATH)) AS CardTransactions		 ",
                    "    FROM [dbo].[CardTransactionBatches] [b]						 ",
                    "    WHERE [b].[Id]=@Id												 ",
                    "    FOR JSON PATH, WITHOUT_ARRAY_WRAPPER);							 ",
                    "SELECT @JSON;														 "
                );

                var batch = await connection.ReadJsonAsync<CardTransactionBatch>(sql, query);
                return batch;
            }
        }
    }
}
