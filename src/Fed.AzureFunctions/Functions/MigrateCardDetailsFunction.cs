using Fed.Api.External.BraintreeService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Common;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Infrastructure.Data.SqlServer.Handlers;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class MigrateCardDetailsFunction
    {
        private const string FuncName = "MigrateCardDetailsFunction";

        [FunctionName(FuncName)]
        public static Task<HttpResponseMessage> Run(
            [HttpTrigger("post", Route = "brainTreeMigration")] HttpRequestMessage req,
            ILogger logger)
            => FunctionRunner.RunWebAsync(logger, FuncName, req, MigrateCardDetails);

        public static async Task<HttpResponseMessage> MigrateCardDetails(HttpRequestMessage req, ServicesBag bag)
        {
            var logger = bag.Logger;

            logger.LogInformation("Retrieving AC Account Number from request...");

            var formData = await req.Content.ReadAsFormDataAsync();
            var acAccountNumber = formData["ACAccountNumber"];

            logger.LogInformation($"Starting card details migration for customer with AC Account Number '{acAccountNumber}'...");

            var braintreeGatewayService =
                new BraintreeGatewayService(
                    bag.Config.Braintree.EnvironmentName,
                    bag.Config.Braintree.MerchantId,
                    bag.Config.Braintree.MerchantAccountId,
                    bag.Config.Braintree.PublicKey,
                    bag.Config.Braintree.PrivateKey,
                    null,
                    logger);

            logger.LogInformation("Looking for Fed customer with matching AC Account Number...");

            var customersHandler = new CustomersHandler(bag.SqlConfig, new RandomIdGenerator());
            var customers = await customersHandler.ExecuteAsync(new GetCustomersQuery(true));

            var customer = customers.SingleOrDefault(
                c => c.ACAccountNumber.HasValue
                && c.ACAccountNumber.Value.ToString() == acAccountNumber);

            if (customer == default(Customer))
            {
                return req.CreateResponse(
                    HttpStatusCode.BadRequest,
                    $"Fed has no customer record with the AC Account Number '{acAccountNumber}'.");
            }

            logger.LogInformation($"Found customer '{customer.CompanyName}' which has the matching AC Account Number '{acAccountNumber}'.");

            var contact = customer.Contacts.First();

            if (contact.CardTokens != null && contact.CardTokens.Count > 0)
            {
                logger.LogInformation($"Customer already has card details set up in Braintree.");

                return req.CreateResponse(
                    HttpStatusCode.OK,
                    $"No card details had to be migrated anymore, because the customer {customer.CompanyName} already has a valid card token registered with Fed.");
            }

            logger.LogInformation($"Starting the migration of card details for the contact '{contact.FirstName} {contact.LastName}' with ID '{contact.Id}'...");

            var cardToken =
                braintreeGatewayService.UpdateCustomer(
                    customer.ACAccountNumber.Value.ToString(),
                    contact.Id);

            if (cardToken == null)
            {
                logger.LogInformation($"No card token found in Braintree for the given customer.");

                return req.CreateResponse(
                    HttpStatusCode.OK,
                    $"No card details found in Braintree for {customer.CompanyName}.");
            }

            logger.LogInformation($"Card token successfully retrieved from Braintree for customer {customer.CompanyName}.");

            cardToken.AddressLine1 = cardToken.AddressLine1 ?? customer.Contacts[0].BillingAddresses[0].AddressLine1;
            cardToken.Postcode = cardToken.Postcode ?? customer.Contacts[0].BillingAddresses[0].Postcode;

            logger.LogInformation("Storing updated card token into Fed database...");

            var cardTokenHandler = new CardTokenHandler(bag.SqlConfig);
            await cardTokenHandler.ExecuteAsync(new CreateCardTokenCommand(contact.Id, cardToken));

            logger.LogInformation("Card token migration completed successfully!");

            return req.CreateResponse(
                HttpStatusCode.OK,
                $"Card token updated in Fed database for customer {customer.CompanyName}.");

        }
    }
}