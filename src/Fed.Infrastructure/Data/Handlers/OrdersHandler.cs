using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class OrdersHandler : IOrdersHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public OrdersHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<Order> ExecuteAsync(GetLastOrderQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = "SELECT TOP 1 * FROM [dbo].[Orders] WHERE [RecurringOrderId] = @RecurringOrderId ORDER BY [DeliveryDate] DESC";
                var lastOrder = await connection.QueryAsync<Order>(sql, query);
                return lastOrder.SingleOrDefault();
            }
        }

        public async Task<IList<OrderSummary>> ExecuteAsync(GetOrderSummaryQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetConfirmedOrderSummary.sql");
                var result =
                    await connection.ReadJsonAsync<IList<OrderSummary>>(
                        sql,
                        new { query.ContactId, FromDate = query.DateRange.From.Value, ToDate = query.DateRange.To.Value });
                return result;
            }
        }

        public async Task<IList<Order>> ExecuteAsync(GetOrdersQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var contactFilter =
                    query.ContactId.HasValue
                    ? " AND [ContactId] = @ContactId"
                    : "";

                var excludeUnpaidJoin =
                    query.ExcludeUnpaid
                    ? " LEFT JOIN [dbo].[CardTransactions] [ct] ON [ct].[OrderId] = [o].[Id] AND [o].[PaymentMethodId] = 1 AND [ct].[CardTransactionStatusId] = 2"
                    : "";

                var excludeUnpaidFilter =
                    query.ExcludeUnpaid
                    ? " AND [ct].[Id] IS NOT NULL OR [o].[PaymentMethodId] <> 1"
                    : "";

                var excludeInvoicedJoin =
                    query.ExcludeInvoiced
                    ? " LEFT JOIN dbo.InvoiceOrders [io] ON [io].[OrderId] = [o].[Id]"
                    : "";

                var excludeInvoicedFilter =
                    query.ExcludeInvoiced
                    ? " AND [io].[InvoiceId] IS NULL "
                    : "";

                var sql = string.Concat(
                    "SELECT [o].*, ",
                    "CASE ",
                    "WHEN LEN(c.ACAccountNumber) > 0 ",
                    "OR EXISTS (SELECT [o2].Id ",
                    "                 FROM dbo.Orders AS o2 ",
                    "                 WHERE o2.DeliveryDate < [o].DeliveryDate ",
                    "                 AND [o2].CustomerId = [o].CustomerId) ",
                    "  THEN 0 ",
                    " ELSE 1 END AS IsFirstOrder, ",
                    " [ts].*, [hub].*, ",
                    " JSON_QUERY((",
                    " SELECT * FROM [dbo].[OrderProducts] ",
                    " WHERE [OrderId] = [o].[Id] ",
                    " FOR JSON PATH)) AS [OrderProducts] ",
                    " FROM [dbo].[Orders] AS [o] ",
                    " INNER JOIN [dbo].[Timeslots] AS [ts] ",
                    " ON [o].[TimeslotId] = [ts].[Id] ",
                    " INNER JOIN [dbo].[Hubs] AS [hub] ",
                    " ON [ts].[HubId] = [hub].[Id] ",
                    " INNER JOIN [dbo].[Customers] AS c ",
                    " ON [c].[Id] = [o].[CustomerId] ",
                    excludeUnpaidJoin,
                    excludeInvoicedJoin,
                    " WHERE [DeliveryDate] >= @From AND [DeliveryDate] <= @To",
                    contactFilter,
                    excludeUnpaidFilter,
                    excludeInvoicedFilter);

                Func<Order, Timeslot, Hub, string, Order> map =
                    (Order, timeslot, hub, json) =>
                    {
                        var orderItems = JsonConvert.DeserializeObject<IList<OrderItem>>(json ?? string.Empty);
                        Order.OrderItems = orderItems;
                        return Order;
                    };

                var contactId = query.ContactId.HasValue ? query.ContactId.Value.ToString() : "";
                var obj = new { From = query.DeliveryDateRange.From.Value, To = query.DeliveryDateRange.To.Value, ContactId = contactId };

                var orders = await connection.QueryAsync(sql, map, obj, splitOn: "Id, Id, Id, OrderProducts");
                return orders.ToList();
            }
        }

        public async Task<Order> ExecuteAsync(GetByIdQuery<Order> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetOrderById");
                var result = await connection.ReadJsonAsync<Order>(sql, query);
                return result;
            }
        }

        public async Task<Guid> ExecuteAsync(CreateCommand<Order> createCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var id = Guid.NewGuid();

                        var orderSql = await SqlQueryReader.GetSqlQueryAsync("InsertOrder");

                        var obj = new
                        {
                            id,
                            createCommand.Object.ShortId,
                            createCommand.Object.ContactId,
                            createCommand.Object.TimeslotId,
                            createCommand.Object.RecurringOrderId,
                            createCommand.Object.OrderName,
                            DeliveryDate = createCommand.Object.DeliveryDate.Value,
                            createCommand.Object.WeeklyRecurrence,
                            createCommand.Object.IsFree
                        };
                      
                        await connection.ExecuteAsync(orderSql, obj, transaction);

                        if (createCommand.Object.OrderItems != null && createCommand.Object.OrderItems.Count > 0)
                            await CreateOrderItems(createCommand.Object.OrderItems, id, connection, transaction);

                        transaction.Commit();

                        return id;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private static async Task CreateOrderItems(IList<OrderItem> item, Guid orderId, SqlConnection connection, SqlTransaction transaction)
        {
            var orderItemsSql = await SqlQueryReader.GetSqlQueryAsync("InsertOrderProduct.sql");
            foreach (var i in item)
            {
                i.OrderId = orderId;
                await connection.ExecuteAsync(orderItemsSql, i, transaction);
            }
        }

        public async Task<Guid> ExecuteAsync(CreateOrderFromRecurringOrderCommand command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var sql = await SqlQueryReader.GetSqlQueryAsync("InsertOrderFromRecurringOrder.sql");

                        var orderItemSql = await SqlQueryReader.GetSqlQueryAsync("InsertOrderProductsFromRecurringOrder.sql");

                        var orderId = Guid.NewGuid();
                        var obj = new { OrderId = orderId, command.ShortId, command.RecurringOrderId, DeliveryDate = command.DeliveryDate.Value };

                        await connection.ExecuteAsync(sql, obj, transaction);
                        await connection.ExecuteAsync(orderItemSql, new { OrderId = orderId, command.RecurringOrderId, command.IsFree }, transaction);

                        transaction.Commit();

                        return orderId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ExecuteAsync(UpdateOrderItemCommand cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("UpdateOrderProduct.sql");
                await connection.ExecuteAsync(sql, cmd.OrderItem);
                return true;
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<OrderItem> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("InsertOrderProduct");
                
                await connection.ExecuteAsync(sql, cmd.Object);
                return true;
            }
        }

        public async Task<bool> ExecuteAsync(CreateCommand<OrderDiscount> createCommand)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("InsertOrderDiscount.sql");

                await connection.ExecuteAsync(sql, createCommand.Object);

                return true;
            }
        }

        public async Task<bool> ExecuteAsync(UpdateOrderDiscountCommand cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("UpdateOrderDiscount");

                await connection.ExecuteAsync(sql, cmd);
                return true;
            }
        }
    }
}