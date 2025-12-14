using Fed.Api.External.MicrosoftTeams;
using Fed.Api.External.XeroService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class RaiseInvoicesFunction
    {
        private const string FuncName = "RaiseInvoicesFunction";

        [FunctionName(FuncName)]
        [Singleton]
        public static Task Run(
            [HttpTrigger(Route = "raiseInvoices")] HttpRequestMessage req,
            ILogger logger)
            => FunctionRunner.RunWebAsync(logger, FuncName, req, RaiseInvoices);

        public static async Task<HttpResponseMessage> RaiseInvoices(HttpRequestMessage request, ServicesBag bag)
        {
            var logger = bag.Logger;
            var fedBot = bag.FedBot;
            var fedClient = bag.FedClient;

            // Load required invoices:
            var toDate = DateTime.Now.Date;
            var fromDate = toDate.AddDays(-6);

            logger.LogInformation($"Getting invoices required for deliveries between {fromDate} and {toDate}...");

            var invoices = await fedClient.GetInvoicesRequiredAsync(fromDate, toDate);

            logger.LogInformation($"{invoices?.Count ?? 0} invoices required between {fromDate} and {toDate}.");

            if (invoices == null || invoices.Count == 0)
                return request.CreateResponse(HttpStatusCode.OK, "No invoices ready to be raised yet.");

            // Connect to Xero:
            logger.LogInformation($"Connecting to Xero service...");

            XeroInvoiceService service = null;

            var settings = new XeroSettings(bag.Config.Xero.ConsumerKey,
                    bag.Config.Xero.ConsumerSecret,
                    bag.Config.Xero.Certificate,
                    bag.Config.Xero.CertificatePassword);

            try
            {
                service = new XeroInvoiceService(settings, logger);
            }
            catch (Exception ex)
            {
                var msg = $"Could not connect to Xero service. Error was {ex.Message}";

                logger.LogError(msg);

                await fedBot.SendMessage(TeamsCard.CreateError(ex, FuncName));

                return request.CreateResponse(HttpStatusCode.InternalServerError, msg);
            }

            logger.LogInformation("Successfully connected to Xero service.");

            logger.LogInformation("Syncing products...");

            var syncProductsService = new XeroProductsSyncService(settings, logger);
            var products = await fedClient.GetProductsAsync();
            await syncProductsService.SyncProducts(products);

            logger.LogInformation("Finished syncing products.");

            logger.LogInformation("Syncing contacts...");
            var syncContactsService = new XeroContactsSyncService(settings, logger);
            var customers = await fedClient.GetCustomersAsync(true);
            await syncContactsService.SyncContacts(customers);

            logger.LogInformation("Finished syncing contacts.");

            // Create Invoices:
            int raisedInvoices = 0;

            foreach (var invoice in invoices)
            {
                Xero.Api.Core.Model.Invoice xeroInvoice = null;

                try
                {
                    xeroInvoice = await service.RaiseInvoice(invoice);
                    raisedInvoices++;
                }
                catch (Exception ex)
                {
                    logger.LogError($"Raising invoice for contact Id {invoice.ContactId} using Xero API. Error was {ex.ToString()}.");
                    await fedBot.SendMessage(TeamsCard.CreateError(ex, FuncName));
                    continue;
                }

                invoice.DateGenerated = DateTime.Now.ToBritishTime();
                invoice.ExternalInvoiceNumber = xeroInvoice.Number;
                invoice.ExternalInvoiceId = xeroInvoice.Id.ToString();

                var id = await fedClient.PostInvoiceAsync(invoice);

                logger.LogInformation($"Created invoice with id {id} for contact Id {invoice.ContactId} in fed repository.");
            }

            // Notify Teams:

            var title = $"{raisedInvoices} out of {invoices.Count} required invoices were successfully raised in Xero.";
            await fedBot.SendMessage(
                TeamsCard.Create(
                    CardType.Invoice,
                    FuncName,
                    $"Invoices have been raised for orders between {fromDate.ToString("yyyy-MM-dd")} and {toDate.ToString("yyyy-MM-dd")}.",
                    title,
                    string.Empty
                ));

            return request.CreateResponse(HttpStatusCode.OK, title);
        }
    }
}
