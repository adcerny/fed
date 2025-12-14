using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Infrastructure.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class DeliveriesHandler : IDeliveriesHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public DeliveriesHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig;
        }

        public async Task<IList<Delivery>> ExecuteAsync(GetDeliveriesQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDeliveries.sql");
                var obj = new { FromDate = query.DateRange.From.Value, ToDate = query.DateRange.To.Value };

                Func<Delivery, string, Delivery> map =
                    (delivery, json) =>
                        {
                            if (!string.IsNullOrEmpty(json))
                            {
                                var orders = JsonConvert.DeserializeObject<IList<Order>>(json);
                                delivery.Orders = orders;
                            }
                            return delivery;
                        };

                var deliveries = await connection.QueryAsync(sql, map, obj, splitOn: "Id, Orders");
                return deliveries.ToList();
            }
        }

        public async Task<Delivery> ExecuteAsync(GetByIdQuery<Delivery> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetDelivery.sql");

                return await connection.ReadJsonAsync<Delivery>(sql, query);
            }
        }

        public async Task<bool> ExecuteAsync(DeleteDeliveryCommand cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("DeleteDeliveries.sql");

                await connection.ExecuteAsync(sql, new { DeliveryDate = cmd.DeliveryDate.Value });

                return true;
            }
        }

        public async Task<IList<Delivery>> ExecuteAsync(CreateCommand<IList<Delivery>> cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var deliverySql = await SqlQueryReader.GetSqlQueryAsync("InsertDelivery.sql");
                        var deliveryOrderSql = await SqlQueryReader.GetSqlQueryAsync("InsertDeliveryOrder.sql");

                        foreach (var delivery in cmd.Object)
                        {
                            var obj =
                                new
                                {
                                    delivery.Id,
                                    delivery.ShortId,
                                    delivery.ContactId,
                                    delivery.DeliveryAddressId,
                                    DeliveryDate = delivery.DeliveryDate.Value,
                                    delivery.TimeslotId,
                                    delivery.EarliestTime,
                                    delivery.LatestTime,
                                    delivery.DeliveryCharge,
                                    delivery.DeliveryCompanyName,
                                    delivery.DeliveryFullName,
                                    delivery.DeliveryAddressLine1,
                                    delivery.DeliveryAddressLine2,
                                    delivery.DeliveryTown,
                                    delivery.DeliveryPostcode,
                                    delivery.DeliveryInstructions,
                                    delivery.LeaveDeliveryOutside,
                                    delivery.PackingStatusId,
                                    delivery.BagCount
                                };

                            await connection.ExecuteAsync(deliverySql, obj, transaction);

                            foreach (var order in delivery.Orders)
                            {
                                var orderObj = new
                                {
                                    DeliveryId = delivery.Id,
                                    OrderId = order.Id
                                };
                                await connection.ExecuteAsync(deliveryOrderSql, orderObj, transaction);
                            }
                        }

                        transaction.Commit();

                        return cmd.Object;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ExecuteAsync(SetDeliveryBagCountCommand cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var sql = await SqlQueryReader.GetSqlQueryAsync("SetDeliveryBagCount.sql");

                        await connection.ExecuteAsync(sql, cmd, transaction);

                        transaction.Commit();

                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ExecuteAsync(SetDeliveryPackingStatusCommand cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var sql = await SqlQueryReader.GetSqlQueryAsync("SetDeliveryPackingStatus.sql");

                        await connection.ExecuteAsync(sql, cmd, transaction);

                        transaction.Commit();

                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}