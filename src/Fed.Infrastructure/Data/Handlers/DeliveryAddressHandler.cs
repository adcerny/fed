using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Entities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class DeliveryAddressHandler : IDeliveryAddressHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public DeliveryAddressHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<Guid> ExecuteAsync(CreateDeliveryAddressCommand command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var deliveryAddressId = Guid.NewGuid();

                        await CustomerTransactions.InsertDeliveryAddress(deliveryAddressId, command, connection, transaction);

                        transaction.Commit();

                        return deliveryAddressId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<DeliveryAddress> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.UpdateDeliveryAddress(command, connection, transaction);
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

        public async Task<bool> ExecuteAsync(DeleteCommand<DeliveryAddress> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.DeleteDeliveryAddress(command.Id, connection, transaction);
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
