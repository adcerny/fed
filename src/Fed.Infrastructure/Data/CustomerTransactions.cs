using Dapper;
using Fed.Core.Common;
using Fed.Core.Data.Commands;
using Fed.Core.Entities;
using Fed.Core.Exceptions;
using Fed.Core.Extensions;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Infrastructure.Data.SqlServer
{
    public static class CustomerTransactions
    {
        public async static Task InsertCustomer(
            Guid customerId,
            string customerShortId,
            CreateCommand<Customer> command,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            var sql = await SqlQueryReader.GetSqlQueryAsync("InsertCustomer.sql");

            var obj =
                new
                {
                    Id = customerId,
                    ShortId = customerShortId,
                    command.Object.CompanyName,
                    command.Object.Website,
                    command.Object.ACAccountNumber,
                    command.Object.FirstDeliveryDate,
                    command.Object.IsInvoiceable,
                    command.Object.OfficeSizeMin,
                    command.Object.OfficeSizeMax,
                    command.Object.IsDeliveryChargeExempt,
                    command.Object.SplitDeliveriesByOrder,
                    command.Object.IsTestAccount,
                    command.Object.Source,
                    command.Object.Notes,
                    command.Object.IsFriend,
                    command.Object.CancellationReason,
                    command.Object.CustomerAgentId,
                    command.Object.CustomerMarketingAttributeId,
                    AccountTypeId = (int)command.Object.AccountType,
                };

            await connection.ExecuteAsync(sql, obj, transaction);

            var idGenerator = new RandomIdGenerator();

            foreach (var contact in command.Object.Contacts.OrEmptyIfNull())
            {
                var contactShortId = $"C{idGenerator.GenerateId()}";
                var createContactCommand = new CreateContactCommand(customerId, contact);
                await InsertContact(Guid.NewGuid(), contactShortId, createContactCommand, connection, transaction);
            }
        }

        public async static Task InsertContact(
            Guid contactId,
            string contactShortId,
            CreateContactCommand command,
            SqlConnection connection,
            SqlTransaction transaction)
        {
            var sql = string.Concat(
                "INSERT INTO [dbo].[Contacts]",
                " ([Id], [ShortId], [CustomerId], [Title], [FirstName], [LastName], [Email], [Phone], [IsMarketingConsented], [PaymentMethodId], [CreatedDate])",
                " VALUES (@Id, @ShortId, @CustomerId, @Title, @FirstName, @LastName, @Email, @Phone, @IsMarketingConsented, @PaymentMethod, GETDATE())");

            var obj = new
            {
                Id = contactId,
                ShortId = contactShortId,
                command.CustomerId,
                command.Contact.Title,
                command.Contact.FirstName,
                command.Contact.LastName,
                command.Contact.Email,
                command.Contact.Phone,
                command.Contact.IsMarketingConsented,
                command.Contact.PaymentMethod
            };

            await connection.ExecuteAsync(sql, obj, transaction);

            if (command.Contact.DeliveryAddresses.OrEmptyIfNull().Count() > 0 && !command.Contact.DeliveryAddresses.Where(a => a.IsPrimary).Any())
                command.Contact.DeliveryAddresses.First().IsPrimary = true;

            foreach (var da in command.Contact.DeliveryAddresses.OrEmptyIfNull())
            {
                var deliveryAddressId = Guid.NewGuid();
                var createDeliveryAddressCommand = new CreateDeliveryAddressCommand(contactId, da);
                await InsertDeliveryAddress(deliveryAddressId, createDeliveryAddressCommand, connection, transaction);
            }

            if (command.Contact.BillingAddresses.OrEmptyIfNull().Count() > 0 && !command.Contact.BillingAddresses.Where(a => a.IsPrimary).Any())
                command.Contact.BillingAddresses.First().IsPrimary = true;

            foreach (var ba in command.Contact.BillingAddresses.OrEmptyIfNull())
            {
                var billingAddressId = Guid.NewGuid();
                var createBillingAddressCommand = new CreateBillingAddressCommand(contactId, ba);

                try
                {
                    await InsertBillingAddress(billingAddressId, createBillingAddressCommand, connection, transaction);
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                        throw new DuplicateCompanyNameException(createBillingAddressCommand.BillingAddress.CompanyName);
                    else
                        throw sqlEx;
                }
            }

            if (command.Contact.CardTokens.OrEmptyIfNull().Count() > 0 && !command.Contact.CardTokens.Where(a => a.IsPrimary).Any())
                command.Contact.CardTokens.First().IsPrimary = true;

            foreach (var ct in command.Contact.CardTokens.OrEmptyIfNull())
            {
                var cardTokenId = Guid.NewGuid();
                var createCreateCardTokenCommand = new CreateCardTokenCommand(contactId, ct);
                await InsertCardToken(cardTokenId, createCreateCardTokenCommand, connection, transaction);
            }
        }

        public async static Task DeleteContact(string contactId, SqlConnection connection, SqlTransaction transaction)
        {
            const string sql = "UPDATE [dbo].[Contacts] SET [IsDeleted] = 1 WHERE Id = @Id";
            var obj = new { Id = contactId };
            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public static async Task InsertDeliveryAddress(Guid id, CreateDeliveryAddressCommand command, SqlConnection connection, SqlTransaction transaction)
        {
            if (command.DeliveryAddress.IsPrimary)
                await ResetPrimaryDeliveryAddress(command.ContactId, connection, transaction);

            var sql = string.Concat(
                "INSERT INTO [dbo].[DeliveryAddresses]",
                "([Id], [ContactId], [IsPrimary], [FullName], [CompanyName], [AddressLine1], [AddressLine2], [Town], [Postcode], [DeliveryInstructions], [Phone], [LeaveDeliveryOutside], [HubId])",
                " VALUES (@Id, @ContactId, @IsPrimary, @FullName, @CompanyName, @AddressLine1, @AddressLine2, @Town, @Postcode, @DeliveryInstructions, @Phone, @LeaveDeliveryOutside, @HubId)");

            var obj = new
            {
                id,
                command.ContactId,
                command.DeliveryAddress.IsPrimary,
                command.DeliveryAddress.FullName,
                command.DeliveryAddress.CompanyName,
                command.DeliveryAddress.AddressLine1,
                command.DeliveryAddress.AddressLine2,
                command.DeliveryAddress.Town,
                command.DeliveryAddress.Postcode,
                command.DeliveryAddress.DeliveryInstructions,
                command.DeliveryAddress.Phone,
                command.DeliveryAddress.LeaveDeliveryOutside,
                command.DeliveryAddress.HubId
            };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public static async Task InsertCardToken(Guid id, CreateCardTokenCommand command, SqlConnection connection, SqlTransaction transaction)
        {
            if (command.CardToken.IsPrimary)
                await ResetPrimaryCardToken(command.ContactId, connection, transaction);

            var sql = string.Concat(
                "INSERT INTO [dbo].[CardTokens]",
                "([Id], [ContactId], [ExpiresYear], [ExpiresMonth], [ObscuredCardNumber], [CardHolderFullName], [AddressLine1], [Postcode], [IsPrimary], [CreatedDate])",
                "VALUES (@Id, @ContactId, @ExpiresYear, @ExpiresMonth, @ObscuredCardNumber, @CardHolderFullName, @AddressLine1, @Postcode, @IsPrimary, GETDATE())");

            var obj = new
            {
                id,
                command.ContactId,
                command.CardToken.ExpiresYear,
                command.CardToken.ExpiresMonth,
                command.CardToken.ObscuredCardNumber,
                command.CardToken.CardHolderFullName,
                command.CardToken.AddressLine1,
                command.CardToken.Postcode,
                command.CardToken.IsPrimary
            };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public static async Task InsertBillingAddress(Guid id, CreateBillingAddressCommand command, SqlConnection connection, SqlTransaction transaction)
        {
            if (command.BillingAddress.IsPrimary)
                await ResetPrimaryBillingAddress(command.ContactId, connection, transaction);

            var sql = string.Concat(
                "INSERT INTO [dbo].[BillingAddresses]",
                "([Id], [ContactId], [IsPrimary], [FullName], [CompanyName], [AddressLine1], [AddressLine2], [Town], [Postcode], [Email], [Phone], [InvoiceReference])",
                "VALUES (@Id, @ContactId, @IsPrimary, @FullName, @CompanyName, @AddressLine1, @AddressLine2, @Town, @Postcode, @Email, @Phone, @InvoiceReference)");

            var obj = new
            {
                id,
                command.ContactId,
                command.BillingAddress.IsPrimary,
                command.BillingAddress.FullName,
                command.BillingAddress.CompanyName,
                command.BillingAddress.AddressLine1,
                command.BillingAddress.AddressLine2,
                command.BillingAddress.Town,
                command.BillingAddress.Postcode,
                command.BillingAddress.Email,
                command.BillingAddress.Phone,
                command.BillingAddress.InvoiceReference
            };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public async static Task UpdateCustomer(UpdateCommand<Customer> command, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = await SqlQueryReader.GetSqlQueryAsync("UpdateCustomer.sql");

            var obj =
                new
                {
                    command.Id,
                    command.Object.CompanyName,
                    command.Object.Website,
                    command.Object.ACAccountNumber,
                    command.Object.IsInvoiceable,
                    command.Object.OfficeSizeMin,
                    command.Object.OfficeSizeMax,
                    command.Object.IsDeliveryChargeExempt,
                    command.Object.SplitDeliveriesByOrder,
                    command.Object.IsTestAccount,
                    command.Object.Source,
                    command.Object.Notes,
                    command.Object.IsFriend,
                    command.Object.CancellationReason,
                    command.Object.CustomerAgentId,
                    command.Object.CustomerMarketingAttributeId,
                    AccountTypeId = (int)command.Object.AccountType
                };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public async static Task UpdateContact(UpdateCommand<Contact> command, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = string.Concat(
                " UPDATE [dbo].[Contacts]" +
                "   SET " +
                "     [Title] = @Title," +
                "     [FirstName] = @FirstName," +
                "     [LastName] = @LastName," +
                "     [Email] = @Email," +
                "     [Phone] = @Phone," +
                "     [IsMarketingConsented] = @IsMarketingConsented," +
                "     [PaymentMethodId] = @PaymentMethod" +
                " WHERE[Id] = @Id");

            var obj = new
            {
                command.Object.Title,
                command.Object.FirstName,
                command.Object.LastName,
                command.Object.Email,
                command.Object.Phone,
                command.Object.IsMarketingConsented,
                command.Object.PaymentMethod,
                command.Id,
            };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public static async Task UpdateDeliveryAddress(UpdateCommand<DeliveryAddress> command, SqlConnection connection, SqlTransaction transaction)
        {
            if (command.Object.IsPrimary)
                await ResetPrimaryDeliveryAddress(command.Object.ContactId, connection, transaction);

            var sql = string.Concat(
                "UPDATE [dbo].[DeliveryAddresses]" +
                "   SET" +
                "      [IsPrimary] = @IsPrimary," +
                "      [FullName] = @FullName," +
                "      [CompanyName] = @CompanyName," +
                "      [AddressLine1] = @AddressLine1," +
                "      [AddressLine2] = @AddressLine2," +
                "      [Town] = @Town," +
                "      [Postcode] = @Postcode," +
                "      [DeliveryInstructions] = @DeliveryInstructions," +
                "      [Phone] = @Phone," +
                "      [LeaveDeliveryOutside] = @LeaveDeliveryOutside" +
                    " WHERE[Id] = @Id");

            var obj = new
            {
                command.Id,
                command.Object.IsPrimary,
                command.Object.FullName,
                command.Object.CompanyName,
                command.Object.AddressLine1,
                command.Object.AddressLine2,
                command.Object.Town,
                command.Object.Postcode,
                command.Object.DeliveryInstructions,
                command.Object.Phone,
                command.Object.LeaveDeliveryOutside
            };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public static async Task ResetPrimaryDeliveryAddress(Guid contactId, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = string.Concat(
                "UPDATE [dbo].[DeliveryAddresses]" +
                "  SET [IsPrimary] = 0" +
                " WHERE ContactId = @ContactId");

            var obj = new
            {
                contactId
            };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public static async Task UpdateCardToken(UpdateCommand<CardToken> command, SqlConnection connection, SqlTransaction transaction)
        {
            if (command.Object.IsPrimary)
                await ResetPrimaryCardToken(command.Object.ContactId, connection, transaction);

            var sql = string.Concat(
                "UPDATE [dbo].[CardTokens]" +
                "  SET" +
                "     [ExpiresYear] = @ExpiresYear," +
                "     [ExpiresMonth] = @ExpiresMonth," +
                "     [ObscuredCardNumber] = @ObscuredCardNumber," +
                "     [CardHolderFullName] = @CardHolderFullName," +
                "     [AddressLine1] = @AddressLine1," +
                "     [Postcode] = @Postcode," +
                "     [IsPrimary] = @IsPrimary" +
                " WHERE Id = @Id");

            var obj = new
            {
                command.Object.ExpiresYear,
                command.Object.ExpiresMonth,
                command.Object.ObscuredCardNumber,
                command.Object.CardHolderFullName,
                command.Object.AddressLine1,
                command.Object.Postcode,
                command.Object.IsPrimary,
                command.Id,
            };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public static async Task ResetPrimaryCardToken(Guid contactId, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = "UPDATE [dbo].[CardTokens] SET [IsPrimary] = 0 WHERE ContactId = @ContactId";

            await connection.ExecuteAsync(sql, new { Contactid = contactId }, transaction);
        }

        public static async Task UpdateBillingAddress(UpdateCommand<BillingAddress> command, SqlConnection connection, SqlTransaction transaction)
        {
            if (command.Object.IsPrimary)
                await ResetPrimaryBillingAddress(command.Object.ContactId, connection, transaction);

            var sql = string.Concat(
                "UPDATE [dbo].[BillingAddresses]" +
                "   SET" +
                "      [IsPrimary] = @IsPrimary," +
                "      [FullName] = @FullName," +
                "      [CompanyName] = @CompanyName," +
                "      [AddressLine1] = @AddressLine1," +
                "      [AddressLine2] = @AddressLine2," +
                "      [Town] = @Town," +
                "      [Postcode] = @Postcode," +
                "      [Email] = @Email," +
                "      [Phone] = @Phone," +
                "      [InvoiceReference] = @InvoiceReference" +
                " WHERE[Id] = @Id");

            var obj = new
            {
                command.Object.IsPrimary,
                command.Object.FullName,
                command.Object.CompanyName,
                command.Object.AddressLine1,
                command.Object.AddressLine2,
                command.Object.Town,
                command.Object.Postcode,
                command.Object.Email,
                command.Object.Phone,
                command.Object.InvoiceReference,
                command.Id
            };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public static async Task ResetPrimaryBillingAddress(Guid contactId, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = string.Concat(
                "UPDATE [dbo].[BillingAddresses]" +
                "  SET [IsPrimary] = 0" +
                " WHERE ContactId = @ContactId");

            var obj = new
            {
                contactId
            };

            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public async static Task DeleteDeliveryAddress(string deliveryAddressId, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = "UPDATE DeliveryAddresses SET IsDeleted = 1, DeletedDate = GETDATE() WHERE Id = @Id";
            var obj = new { Id = deliveryAddressId };
            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public async static Task DeleteCardToken(string cardTokenId, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = "DELETE FROM [dbo].[CardTokens] WHERE [Id] = @Id";
            var obj = new { Id = cardTokenId };
            await connection.ExecuteAsync(sql, obj, transaction);
        }

        public async static Task DeleteBillingAddress(string deliveryAddressId, SqlConnection connection, SqlTransaction transaction)
        {
            var sql = "DELETE FROM [dbo].[BillingAddresses] WHERE [Id] = @Id";
            var obj = new { Id = deliveryAddressId };
            await connection.ExecuteAsync(sql, obj, transaction);
        }
    }
}