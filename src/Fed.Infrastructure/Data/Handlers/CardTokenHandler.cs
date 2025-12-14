using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Entities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class CardTokenHandler : ICardTokenHandler
    {
        private readonly ISqlServerConfig _sqlConfig;

        public CardTokenHandler(ISqlServerConfig sqlConfig)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
        }

        public async Task<Guid> ExecuteAsync(CreateCardTokenCommand command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var cardTokenId = command.CardToken.Id == Guid.Empty ? Guid.NewGuid() : command.CardToken.Id;

                        await CustomerTransactions.InsertCardToken(cardTokenId, command, connection, transaction);

                        transaction.Commit();

                        return cardTokenId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<CardToken> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.UpdateCardToken(command, connection, transaction);
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

        public async Task<bool> ExecuteAsync(DeleteCommand<CardToken> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.DeleteCardToken(command.Id, connection, transaction);
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
