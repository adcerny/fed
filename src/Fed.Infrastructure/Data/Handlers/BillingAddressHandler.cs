using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Entities;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class BillingAddressHandler : IBillingAddressHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public BillingAddressHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<Guid> ExecuteAsync(CreateBillingAddressCommand command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var deliveryAddressId = Guid.NewGuid();

                        await CustomerTransactions.InsertBillingAddress(deliveryAddressId, command, connection, transaction);

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

        public async Task<bool> ExecuteAsync(UpdateCommand<BillingAddress> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.UpdateBillingAddress(command, connection, transaction);

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

        public async Task<bool> ExecuteAsync(DeleteCommand<BillingAddress> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.DeleteBillingAddress(command.Id, connection, transaction);

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
