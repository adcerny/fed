using Fed.Core.Common.Interfaces;
﻿using Dapper;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Exceptions;
using Fed.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer.Handlers
{
    public class CustomersHandler : ICustomersHandler
    {
        private readonly ISqlServerConfig _sqlConfig;
        private readonly IIdGenerator _idGenerator;

        public CustomersHandler(ISqlServerConfig sqlConfig, IIdGenerator idGenerator)
        {
            _sqlConfig = sqlConfig ?? throw new ArgumentNullException(nameof(sqlConfig));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public async Task<Customer> ExecuteAsync(GetByIdQuery<Customer> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetCustomerById.sql");

                return await connection.ReadJsonAsync<Customer>(sql, query);
            }
        }

        public async Task<FullCustomerInfo> ExecuteAsync(GetByIdQuery<FullCustomerInfo> query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetFullCustomerInfoById.sql");

                return await connection.ReadJsonAsync<FullCustomerInfo>(sql, query);
            }
        }

        public async Task<Customer> ExecuteAsync(GetCustomerByContactIdQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetCustomerByContactId.sql");

                return await connection.ReadJsonAsync<Customer>(sql, query);
            }
        }

        public Task<IList<Customer>> ExecuteAsync(GetCustomersQuery query)
        {
            return query.IncludeContacts
                ? GetAllCustomersWithContactsAsync()
                : GetAllCustomersAsync();
        }

        private async Task<IList<Customer>> GetAllCustomersAsync()
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetAllCustomersWithoutContacts.sql");

                return (await connection.ReadJsonAsync<IList<Customer>>(sql));
            }
        }

        private async Task<IList<Customer>> GetAllCustomersWithContactsAsync()
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("GetAllCustomersWithContacts.sql");

                return await connection.ReadJsonAsync<IList<Customer>>(sql);
            }
        }

        public async Task<Customer> ExecuteAsync(CreateCommand<Customer> command)
        {
            var customerId = Guid.NewGuid();
            var customerShortId = $"B{_idGenerator.GenerateId()}";

            try
            {
                return await ExecuteAsync(customerId, customerShortId, command);
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                {
                    return await ExecuteAsync(command);
                }

                throw;
            }
        }

        private async Task<Customer> ExecuteAsync(Guid customerId, string customerShortId, CreateCommand<Customer> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var primaryEmail = command.Object.PrimaryContact.Email;
                        var existingCustomerId = await ExecuteAsync(new GetCustomerIdByEmailQuery(primaryEmail));

                        if (existingCustomerId.HasValue)
                            throw new DuplicateEmailAddresssException(primaryEmail, existingCustomerId.Value);

                        await CustomerTransactions.InsertCustomer(customerId, customerShortId, command, connection, transaction);

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }

            return await ExecuteAsync(new GetByIdQuery<Customer>(customerId));
        }

        public async Task<bool> ExecuteAsync(UpdateCommand<Customer> command)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                await connection.OpenAsync();

                using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        await CustomerTransactions.UpdateCustomer(command, connection, transaction);

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

        public async Task<Guid?> ExecuteAsync(GetCustomerIdByEmailQuery query)
        {
            using (var connection = new SqlConnection(_sqlConfig.ConnectionString))
            {
                var sql = await SqlQueryReader.GetSqlQueryAsync("IsEmailAlreadyInUse.sql");

                return await connection.QuerySingleOrDefaultAsync<Guid?>(sql, query);
            }
        }
    }
}
