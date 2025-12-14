using Fed.Api.External.MicrosoftTeams;
using Fed.Api.External.XeroService;
using Fed.AzureFunctions.Entities;
using Fed.Core.Converters;
using Fed.Core.Entities;
using Fed.Core.Extensions;
using Fed.Web.Service.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class XeroInvoiceOrchestrator
    {
        private const string FuncName = "XeroInvoiceOrchestrator_HttpStart";

        [FunctionName(FuncName)]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Function, methods: "post", Route = "orchestrators/{functionName}/{instanceId}")] HttpRequestMessage req,
            [DurableClient]IDurableOrchestrationClient starter,
            string functionName,
            string instanceId,
            ILogger logger)
        {

            //check that instance is not already running
            var existingInstance = await starter.GetStatusAsync(instanceId);
            if (existingInstance != null &&
                (existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Running ||
                 existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Pending ||
                 existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.ContinuedAsNew))
                return req.CreateErrorResponse(
                    HttpStatusCode.Conflict,
                    $"An XeroInvoiceOrchestrator instance with ID '{instanceId}' already exists with status '{existingInstance.RuntimeStatus}.'");

            // Function input comes from the request content.
            await starter.StartNewAsync(functionName, instanceId);
            logger.LogInformation($"Started orchestration with ID = '{instanceId}'.");
            return starter.CreateCheckStatusResponse(req, instanceId);
        }


        [FunctionName("XeroInvoiceOrchestrator")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context
            , ILogger log
            )
        {
            var outputs = new List<string>();

            await context.CallActivityAsync("XeroInvoiceOrchestrator_SyncContactsAndProducts", null);

            var invoicesJson = await context.CallActivityAsync<string>("XeroInvoiceOrchestrator_GetInvoicesRequired", new object());

            var toDate = DateTime.Now.Date;
            var fromDate = toDate.AddDays(-6);
            log.LogInformation($"Getting invoices required for deliveries between {fromDate} and {toDate}...");

            var config = Config.LoadFromEnvironment();

            var invoices = JsonConvert.DeserializeObject<List<Invoice>>(invoicesJson, GetSerializerSettings());
            foreach (var invoice in invoices.OrEmptyIfNull())
            {
                log.LogInformation($"Calling RaiseInvoice for contact {invoice.ContactId}...");

                var invoiceJson = JsonConvert.SerializeObject(invoice, GetSerializerSettings());
                outputs.Add(await context.CallActivityAsync<string>("XeroInvoiceOrchestrator_RaiseInvoice", invoiceJson));
            }

            var fedBot = FedBot.Create(log, config.TeamsWebhookUrl);
            var sucessfulInvoices = outputs?.Where(s => s != null)?.ToList();

            var title = $"{sucessfulInvoices?.Count ?? 0} out of {invoices?.Count ?? 0} required invoices were successfully raised in Xero.";

            await context.CallActivityAsync("XeroInvoiceOrchestrator_SendSummary", title);
            return outputs;
        }

        [FunctionName("XeroInvoiceOrchestrator_RaiseInvoice")]
        public static async Task<string> RaiseInvoice([ActivityTrigger] string invoiceJson, ILogger log)
        {
            
            var invoice = JsonConvert.DeserializeObject<Invoice>(invoiceJson, GetSerializerSettings());

            var config = Config.LoadFromEnvironment();
            var fedBot = FedBot.Create(log, config.TeamsWebhookUrl);

            var settings = new XeroSettings(config.Xero.ConsumerKey,
                    config.Xero.ConsumerSecret,
                    config.Xero.Certificate,
                    config.Xero.CertificatePassword);

            XeroInvoiceService service = null;
            try
            {
                service = new XeroInvoiceService(settings, log);
            }
            catch (Exception ex)
            {
                var msg = $"Could not connect to Xero service. Error was {ex.Message}";

                log.LogError(msg);

                await fedBot.SendMessage(TeamsCard.CreateError(ex, FuncName));

                return null;
            }

            Xero.Api.Core.Model.Invoice xeroInvoice = null;

            try
            {
                xeroInvoice = await service.RaiseInvoice(invoice);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error raising invoice for contact Id {invoice.ContactId} using Xero API. ";
                log.LogError($"{errorMsg} Error was {ex.ToString()}");
                await fedBot.SendMessage(TeamsCard.CreateWarning(ex, FuncName, errorMsg));
                return null;
            }

            invoice.DateGenerated = DateTime.Now.ToBritishTime();
            invoice.ExternalInvoiceNumber = xeroInvoice.Number;
            invoice.ExternalInvoiceId = xeroInvoice.Id.ToString();

            var fedWebClient = new FedWebClient(
                    log,
                    new HttpClient(),
                    config.FedWebServiceUrl);

            var id = await fedWebClient.PostInvoiceAsync(invoice);

            log.LogInformation($"Created invoice with id {id} for contact Id {invoice.ContactId} in fed repository.");

            return invoice.ExternalInvoiceNumber;
        }

        [FunctionName("XeroInvoiceOrchestrator_GetInvoicesRequired")]
        public static async Task<string> GetInvoicesRequired([ActivityTrigger] string name, ILogger log)
        {
            var toDate = DateTime.Now.Date;
            var fromDate = toDate.AddDays(-6);
            log.LogInformation($"Getting invoices required for deliveries between {fromDate} and {toDate}...");

            var config = Config.LoadFromEnvironment();

            var fedWebClient = new FedWebClient(
                    log,
                    new HttpClient(),
                    config.FedWebServiceUrl);

            // Load required invoices:
            var invoices = await fedWebClient.GetInvoicesRequiredAsync(fromDate, toDate);
            return JsonConvert.SerializeObject(invoices, GetSerializerSettings());
        }

        [FunctionName("XeroInvoiceOrchestrator_SyncContactsAndProducts")]
        public static async Task SyncContacts([ActivityTrigger] string name, ILogger log)
        {
            var config = Config.LoadFromEnvironment();

            var fedWebClient = new FedWebClient(
                    log,
                    new HttpClient(),
                    config.FedWebServiceUrl);

            var fedBot = FedBot.Create(log, config.TeamsWebhookUrl);

            var settings = new XeroSettings(config.Xero.ConsumerKey,
                    config.Xero.ConsumerSecret,
                    config.Xero.Certificate,
                    config.Xero.CertificatePassword);

            log.LogInformation("Syncing products...");

            var syncProductsService = new XeroProductsSyncService(settings, log);
            var products = await fedWebClient.GetProductsAsync();
            try
            {
                await syncProductsService.SyncProducts(products);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error syncing products using Xero API. ";
                log.LogError($"{errorMsg} Error was {ex.ToString()}");
                await fedBot.SendMessage(TeamsCard.CreateWarning(ex, FuncName, errorMsg));
            }
            log.LogInformation("Finished syncing products.");

            log.LogInformation("Syncing contacts...");
            var syncContactsService = new XeroContactsSyncService(settings, log);
            var customers = await fedWebClient.GetCustomersAsync(true);
            try
            {
                await syncContactsService.SyncContacts(customers);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error syncing contacts using Xero API. ";
                log.LogError($"{errorMsg} Error was {ex.ToString()}");
                await fedBot.SendMessage(TeamsCard.CreateWarning(ex, FuncName, errorMsg));
            }
            log.LogInformation("Finished syncing contacts.");
        }


        [FunctionName("XeroInvoiceOrchestrator_SendSummary")]
        public static async Task SendTeamsMessage([ActivityTrigger] string title, ILogger log)
        {
            var config = Config.LoadFromEnvironment();

            var fedBot = FedBot.Create(log, config.TeamsWebhookUrl);

            await fedBot.SendMessage(
                TeamsCard.Create(
                    CardType.Invoice,
                    FuncName,
                    "Invoices for this week have been raised.",
                    title,
                    string.Empty
                ));
        }

        public static JsonSerializerSettings GetSerializerSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new DateJsonConverter());
            return settings;
        }
}
}