using Dapper;
using Fed.Core.Common;
using Fed.Core.Common.Interfaces;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class ContactsHandler : IContactsHandler
    {
        private readonly ISqlServerConfig _sqlConfig;
        private readonly IIdGenerator _idGenerator;

        public ContactsHandler(ISqlServerConfig sqlConfig, IIdGenerator idGenerator)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public async Task<Contact> ExecuteAsync(GetByIdQuery<Contact> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = string.Concat(
                    "DECLARE @JSON NVARCHAR(MAX) = (",
                    "	SELECT *,",
                    "	JSON_QUERY ((SELECT * FROM [dbo].[DeliveryAddresses] AS [da] WHERE [da].[ContactId] = [co].[Id] AND [da].[IsDeleted] = 0 FOR JSON PATH)) AS [DeliveryAddresses],",
                    "	JSON_QUERY ((SELECT * FROM [dbo].[BillingAddresses]  AS [ba] WHERE [ba].[ContactId] = [co].[Id] FOR JSON PATH)) AS [BillingAddresses],",
                    "	JSON_QUERY ((SELECT * FROM [dbo].[CardTokens]        AS [ct] WHERE [ct].[ContactId] = [co].[Id] FOR JSON PATH)) AS [CardTokens]",
                    "	FROM [dbo].[Contacts] AS [co]",
                    "	WHERE [co].[Id] = @Id",
                    "	FOR JSON PATH, WITHOUT_ARRAY_WRAPPER",
                    ") SELECT @JSON");

                return await connection.ReadJsonAsync<Contact>(sql, query);
            }
        }

        public async Task<IList<Contact>> ExecuteAsync(GetContactsQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                const string sql = "SELECT * FROM [dbo].[Contacts] WHERE [CustomerId] = @CustomerId";

                return (await connection.QueryAsync<Contact>(sql, query)).ToList();
            }
        }

        public async Task<Guid> ExecuteAsync(CreateContactCommand command)
        {
            var contactId = Guid.NewGuid();
            var contactShortId = $"C{_idGenerator.GenerateId()}";

            try
            {
                return await ExecuteAsync(contactId, contactShortId, command);
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                {
                    return await ExecuteAsync(command);
                }

                throw sqlEx;
            }
        }

        private async Task<Guid> ExecuteAsync(Guid contactId, string contactShortId, CreateContactCommand command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.InsertContact(contactId, contactShortId, command, connection, transaction);
                        transaction.Commit();
                        return contactId;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<Contact> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.UpdateContact(command, connection, transaction);
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

        public async Task<bool> ExecuteAsync(DeleteCommand<Contact> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.DeleteContact(command.Id, connection, transaction);
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

        public async Task<bool> ExecuteAsync(UpdateMarketingConsentCommand cmd)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var sql = await SqlQueryReader.GetSqlQueryAsync("UpdateMarketingConsent.sql");
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
