using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Exceptions;
using Fed.Core.ValueTypes;
using Fed.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class RecurringOrdersHandler : IRecurringOrdersHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public RecurringOrdersHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<IList<RecurringOrder>> ExecuteAsync(GetRecurringOrdersQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetRecurringOrders");
                var obj =
                    new
                    {
                        FromDate = query.DateRange.From.Value,
                        ToDate = query.DateRange.To.Value,
                        query.IncludeExpired,
                        query.IncludeFromDemoAccounts,
                        query.IncludeFromCancelledAccounts,
                        query.IncludeFromDeletedAccounts,
                        query.IncludeFromPausedAccounts,
                        query.ContactId
                    };

                var result = await connection.ReadJsonAsync<IList<RecurringOrder>>(sql, obj);
                return result;
            }
        }

        public async Task<RecurringOrder> ExecuteAsync(GetByIdQuery<RecurringOrder> query)
        {
            var orders = await ExecuteAsync(new GetByIdsQuery<RecurringOrder>(new List<String> { query.Id }));
            return orders?.Single();
        }

        public async Task<IList<RecurringOrder>> ExecuteAsync(GetByIdsQuery<RecurringOrder> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {

                var ut = new DataTable("IdTable");
                ut.Columns.Add("Id");
                foreach (var id in query.Ids)
                    ut.Rows.Add(id);

                var sql = await SqlQueryReader.GetSqlQueryAsync("GetRecurringOrdersByIds");
                var result = await connection.ReadJsonAsync<IList<RecurringOrder>>(sql, new { ut = ut.AsTableValuedParameter("IdTable") });
                return result;
            }
        }

        public async Task<RecurringOrder> ExecuteAsync(CreateCommand<RecurringOrder> cmd)
        {
            if (cmd.Object.OrderItems == null || cmd.Object.OrderItems.Count == 0)
                throw new OrderHasNoItemsException(cmd.Object.Name);

            var recurringOrderId = Guid.NewGuid();

            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var saveRecurringOrderStatement =
                            string.Concat(
                                "INSERT INTO [dbo].[RecurringOrders] ",
                                "([Id], [Name], [ContactId], [DeliveryAddressId], [BillingAddressId], [StartDate], [EndDate], [WeeklyRecurrence], [TimeslotId], [CreatedDate], [LastUpdatedDate], [DeletedDate], [IsDeleted], [IsFree]) ",
                                "VALUES (@Id, @Name, @ContactId, @DeliveryAddressId, @BillingAddressId, @StartDate, @EndDate, @WeeklyRecurrence, @TimeslotId, GETDATE(), GETDATE(), NULL, 0, @IsFree)");

                        var saveRecurringOrderProductStatement =
                            string.Concat(
                                "INSERT INTO [dbo].[RecurringOrderProducts] ",
                                "([RecurringOrderId], [ProductId], [Quantity], [AddedDate]) ",
                                "VALUES (@RecurringOrderId, @ProductId, @Quantity, GETDATE())");

                        var endDate =
                            cmd.Object.WeeklyRecurrence == WeeklyRecurrence.OneOff
                            ? cmd.Object.StartDate.Value
                            : cmd.Object.EndDate.HasValue
                                ? cmd.Object.EndDate.Value.Value
                                : Date.MaxDate.Value;

                        await connection.ExecuteAsync(
                            saveRecurringOrderStatement,
                            new
                            {
                                Id = recurringOrderId,
                                cmd.Object.Name,
                                cmd.Object.ContactId,
                                cmd.Object.DeliveryAddressId,
                                cmd.Object.BillingAddressId,
                                StartDate = cmd.Object.StartDate.Value,
                                EndDate = endDate,
                                cmd.Object.WeeklyRecurrence,
                                cmd.Object.TimeslotId,
                                cmd.Object.IsFree
                            },
                            transaction);

                        foreach (var orderItem in cmd.Object.OrderItems)
                        {
                            await connection.ExecuteAsync(
                                saveRecurringOrderProductStatement,
                                new { RecurringOrderId = recurringOrderId, orderItem.ProductId, orderItem.Quantity },
                                transaction);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            var query = new GetByIdQuery<RecurringOrder>(recurringOrderId);
            return await ExecuteAsync(query);
        }

        public async Task<RecurringOrder> ExecuteAsync(UpdateCommand<RecurringOrder> cmd)
        {
            if (cmd.Object.OrderItems == null || cmd.Object.OrderItems.Count == 0)
                throw new OrderHasNoItemsException(cmd.Object.Name);

            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var updateRecurringOrderStatement =
  string.Concat(
      "UPDATE [dbo].[RecurringOrders] ",
      "SET [Name] = @Name, [ContactId] = @ContactId, [StartDate] = @StartDate, [EndDate] = @EndDate, [WeeklyRecurrence] = @WeeklyRecurrence, [TimeslotId] = @TimeslotId, [LastUpdatedDate] = GETDATE(), [DeliveryAddressId] = @DeliveryAddressId , [IsFree] = @IsFree ",
      "WHERE [Id] = @Id");

                        var deleteRecurringOrderProductsStatement =
                            "DELETE FROM [dbo].[RecurringOrderProducts] WHERE [RecurringOrderId] = @RecurringOrderId";

                        var saveRecurringOrderProductStatement =
                            string.Concat(
                                "INSERT INTO [dbo].[RecurringOrderProducts] ",
                                "([RecurringOrderId], [ProductId], [Quantity], [AddedDate]) ",
                                "VALUES (@RecurringOrderId, @ProductId, @Quantity, GETDATE())");

                        var startDate = cmd.Object.StartDate.Value;

                        // The cogworks website is sending a null enddate for recurring orders
                        // which have no enddate. In this case we set it to the max date.
                        var endDate =
                            cmd.Object.EndDate.HasValue
                                ? cmd.Object.EndDate.Value.Value
                                : Date.MaxDate.Value;

                        // Special case for One off orders:
                        // If the updated order has an end date before the start date
                        // then it means that this order should get cancelled.
                        // In this case we set both, the start date and end date to the lower date.
                        // Otherwise we set the end date to the start date.
                        // Either way, with one off orders the start and end date always have to be same.
                        if (cmd.Object.WeeklyRecurrence == WeeklyRecurrence.OneOff)
                        {
                            if (cmd.Object.EndDate < cmd.Object.StartDate)
                            {
                                startDate = cmd.Object.EndDate.Value.Value;
                                endDate = cmd.Object.EndDate.Value.Value;
                            }
                            else
                            {
                                endDate = startDate;
                            }
                        }

                        await connection.ExecuteAsync(
                            updateRecurringOrderStatement,
                            new
                            {
                                cmd.Id,
                                cmd.Object.Name,
                                cmd.Object.ContactId,
                                StartDate = cmd.Object.StartDate.Value,
                                EndDate = endDate,
                                cmd.Object.WeeklyRecurrence,
                                cmd.Object.TimeslotId,
                                cmd.Object.DeliveryAddressId,
                                cmd.Object.IsFree
                            },
                            transaction);

                        await connection.ExecuteAsync(
                            deleteRecurringOrderProductsStatement,
                            new { RecurringOrderId = cmd.Id },
                            transaction);

                        foreach (var orderItem in cmd.Object.OrderItems)
                        {
                            if (orderItem.Quantity > 0)
                                await connection.ExecuteAsync(
                                    saveRecurringOrderProductStatement,
                                    new { RecurringOrderId = cmd.Id, orderItem.ProductId, orderItem.Quantity },
                                    transaction);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            var query = new GetByIdQuery<RecurringOrder>(cmd.Id);
            return await ExecuteAsync(query);
        }

        public async Task<bool> ExecuteAsync(DeleteCommand<RecurringOrder> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                string sql = string.Concat(
                  "UPDATE dbo.RecurringOrders  ",
                  " SET IsDeleted = 1,         ",
                  "     DeletedDate = GETDATE()",
                  " WHERE Id = @Id              " +
                  " AND IsDeleted = 0           ");

                int rows = await connection.ExecuteAsync(sql, new { cmd.Id });
                return rows.Equals(1);
            }
        }
    }
}
