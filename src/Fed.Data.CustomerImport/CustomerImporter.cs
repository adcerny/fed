using Fed.Api.External.BraintreeService;
using Fed.Core.Common;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Infrastructure.Data.SqlServer;
using Fed.Infrastructure.Data.SqlServer.Handlers;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Data.CustomerImport
{
    public class CustomerImporter
    {
        private static void ValidateHeaders(IDictionary<string, int> headers)
        {
            var requiredHeaders = new[]
            {
                "Acct number",
                "Customer name",
                "Contact Title",
                "Contact First Name",
                "Contact Last Name",
                "Post code",
                "Customer address 1",
                "Customer address 2",
                "Billing Address line 1",
                "Billing Address line 2",
                "Billing Postcode",
                "Contact email",
                "Secondary email",
                "Primary Ph #",
                "Secondary Ph #",
                "Delivery Instructions",
                "Payment Method",
                "Website"
            };

            foreach (var requiredHeader in requiredHeaders)
            {
                if (!headers.ContainsKey(requiredHeader))
                {
                    throw new Exception($"Column '{requiredHeader}' could not be found");
                }
            }
        }

        private static Customer ParseCsvRow(IDictionary<string, int> headers, string[] values)
        {
            ValidateHeaders(headers);

            var acctNumber = values[headers["Acct number"]].Trim();
            var acAccountNumber =
                string.IsNullOrEmpty(acctNumber)
                ? (int?)null
                : int.Parse(acctNumber);

            var companyName = values[headers["Customer name"]].Trim();
            var title = values[headers["Contact Title"]].Trim();
            var firstName = values[headers["Contact First Name"]].Trim();
            var lastName = values[headers["Contact Last Name"]].Trim();
            var fullName = $"{title} {firstName} {lastName}";

            var postcode = values[headers["Post code"]].Trim();
            var addressLine1 = values[headers["Customer address 1"]].Trim();
            var addressLine2 = values[headers["Customer address 2"]].Trim();

            var billingAddressLine1 = values[headers["Billing Address line 1"]].Trim();
            var billingAddressLine2 = values[headers["Billing Address line 2"]].Trim();
            var billingPostcode = values[headers["Billing Postcode"]].Trim();

            var email1 = values[headers["Contact email"]].Trim();
            var email2 = values[headers["Secondary email"]].Trim();

            var phone1 = values[headers["Primary Ph #"]].Trim();
            var phone2 = values[headers["Secondary Ph #"]].Trim();

            var deliveryInstructions = values[headers["Delivery Instructions"]].Trim();

            // Default Billing details to null if not set
            email2 = string.IsNullOrWhiteSpace(email2) ? null : email2;
            phone2 = string.IsNullOrWhiteSpace(phone2) ? null : phone2;
            billingAddressLine1 = string.IsNullOrWhiteSpace(billingAddressLine1) ? null : billingAddressLine1;
            billingAddressLine2 = string.IsNullOrWhiteSpace(billingAddressLine2) ? null : billingAddressLine2;
            billingPostcode = string.IsNullOrWhiteSpace(billingPostcode) ? null : billingPostcode;

            var rawPaymentMethod = values[headers["Payment Method"]].Trim();

            var paymentMethod = PaymentMethod.Invoice;

            if (!string.IsNullOrWhiteSpace(rawPaymentMethod))
            {
                if (rawPaymentMethod.ToLower().Equals("card payment")) paymentMethod = PaymentMethod.Card;
                else if (rawPaymentMethod.ToLower().Equals("direct debit")) paymentMethod = PaymentMethod.DirectDebit;
            }

            var customer = new Customer(
                Guid.Empty,
                string.Empty,
                companyName,
                values[headers["Website"]],
                acAccountNumber,
                true,
                10,
                100,
                false,
                false,
                false,
                AccountType.Standard,
                "Imported by Customer Support",
                null,
                false,
                null);

            var deliveryAddress = new DeliveryAddress(
                Guid.Empty,
                Guid.Empty,
                true,
                fullName,
                companyName,
                addressLine1,
                addressLine2,
                // Town is not specified in customer master spreadsheet
                "",
                postcode,
                deliveryInstructions,
                "",
                false,
                Guid.Empty);

            var billingAddress = new BillingAddress(
                Guid.Empty,
                Guid.Empty,
                true,
                fullName,
                companyName,
                billingAddressLine1 ?? addressLine1,
                billingAddressLine2 ?? addressLine2,
                // Town is not specified in customer master spreadsheet
                "",
                billingPostcode ?? postcode,
                email2 ?? email1,
                phone2 ?? phone1,
                null);

            var contact = new Contact(
                Guid.Empty,
                string.Empty,
                Guid.Empty,
                title,
                firstName,
                lastName,
                email1,
                phone1,
                false,
                false,
                (int)paymentMethod,
                DateTime.UtcNow,
                new[] { deliveryAddress },
                new[] { billingAddress },
                null);

            customer.Contacts.Add(contact);

            return customer;
        }

        private static IList<Customer> ParseCustomers(string csvFileName) => CsvParser.ParseCsv(csvFileName, ParseCsvRow);

        public static async Task<int> ImportFromCsvAsync(string csvFileName, string connectionString, BraintreeConfig braintreeConfig)
        {
            var customers = ParseCustomers(csvFileName);

            var sqlConfig = new SqlServerConfig(connectionString);

            var hubsHandler = new HubsHandler(sqlConfig);
            var customersHandler = new CustomersHandler(sqlConfig, new RandomIdGenerator());
            var cardTokenHandler = new CardTokenHandler(sqlConfig);

            var hubs = await hubsHandler.ExecuteAsync(new GetHubsQuery());
            var hubId = hubs[0].Id;

            var braintreeGatewayService = new BraintreeGatewayService(
                braintreeConfig.Environment,
                braintreeConfig.MerchantId,
                braintreeConfig.MerchantAccountId,
                braintreeConfig.PublicKey,
                braintreeConfig.PrivateKey,
                null,
                NullLogger.Instance);

            foreach (var customer in customers)
            {
                try
                {
                    var contact = customer.Contacts[0];
                    contact.DeliveryAddresses[0].HubId = hubId;

                    // 1. Create the customer first in the db
                    var createCmd = new CreateCommand<Customer>(customer);
                    var createdCustomer = await customersHandler.ExecuteAsync(createCmd);
                    var createdContactId = createdCustomer.Contacts[0].Id;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Customer {customer.CompanyName} has been created in the Fed database.");
                    Console.ResetColor();

                    // 2. Get a new card token from Braintree
                    if (!customer.ACAccountNumber.HasValue)
                        continue;

                    var cardToken =
                        braintreeGatewayService.UpdateCustomer(
                            customer.ACAccountNumber.Value.ToString(),
                            createdContactId);

                    if (cardToken != null)
                    {
                        Console.WriteLine($"Card token successfully retrieved from Braintree for customer {customer.CompanyName}.");

                        cardToken.AddressLine1 = cardToken.AddressLine1 ?? customer.Contacts[0].BillingAddresses[0].AddressLine1;
                        cardToken.Postcode = cardToken.Postcode ?? customer.Contacts[0].BillingAddresses[0].Postcode;
                    }

                    // 3. Save the card token for the customer in the db
                    try
                    {
                        await cardTokenHandler.ExecuteAsync(new CreateCardTokenCommand(createdContactId, cardToken));

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Card token updated in Fed database for customer {customer.CompanyName}.");
                        Console.ResetColor();
                    }
                    catch (Exception ex)
                    {
                        if (customer.Contacts[0].PaymentMethod == PaymentMethod.Card)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                            Console.WriteLine($"[Braintree Error]: An error occurred when getting the Braintree card token for customer '{customer.CompanyName}'.");
                            Console.WriteLine($"[Braintree Error]: {ex.Message}.");
                            Console.ResetColor();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine($"[Import Error]: An error occured when importing a customer '{customer.CompanyName}' into Fed.");
                    Console.WriteLine($"[Import Error]: {ex.Message}.");
                    Console.ResetColor();
                }

            }

            return customers.Count;
        }
    }
}
