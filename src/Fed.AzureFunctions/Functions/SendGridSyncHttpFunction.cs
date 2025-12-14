using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class SendGridSyncHttpFunction
    {
        private const string FuncName = "SendGridSyncHttpFunction";

        [FunctionName(FuncName)]
        public static Task<HttpResponseMessage> Run(
            [HttpTrigger(Route = "sendGridSync")] HttpRequestMessage req,
            [Queue(QueueNames.SendGridSync)] IAsyncCollector<string> sendGridQueue,
            ILogger logger)
            => FunctionRunner.RunWebWithArgsAsync(
                logger,
                FuncName,
                req,
                sendGridQueue,
                SendGridSync);

        public static async Task<HttpResponseMessage> SendGridSync((HttpRequestMessage, IAsyncCollector<string>) args, ServicesBag bag)
        {
            var (req, queue) = args;
            var logger = bag.Logger;
            var content = await req.Content.ReadAsStringAsync();
            logger.LogInformation($"Received sync event from SendGrid: {content}.");

            var sendGridEvents = JsonConvert.DeserializeObject<IList<SendGridEvent>>(content);

            if (sendGridEvents == null || sendGridEvents.Count == 0)
            {
                logger.LogInformation("No event to synchronise.");
                return
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("No event to synchronise.")
                    };
            }

            logger.LogInformation($"{sendGridEvents.Count} events sent for synchronisation. Starting to process events now...");

            foreach (var sendGridEvent in sendGridEvents)
            {
                logger.LogInformation($"Email Address: {sendGridEvent.Email}");
                logger.LogInformation($"Has unsubscribed: {sendGridEvent.IsUnsubscribed}");

                if (!sendGridEvent.IsUnsubscribed && !sendGridEvent.IsResubscribed)
                    continue; // ToDo: Log false events

                var sendGridSyncInstruction = new SendGridSyncInstruction
                {
                    EmailAddress = sendGridEvent.Email,
                    Event =
                        sendGridEvent.IsUnsubscribed
                            ? SendGridSyncInstruction.SyncEvent.Unsubscribe
                            : SendGridSyncInstruction.SyncEvent.Subscribe,
                    Source = SendGridSyncInstruction.SyncSource.SendGrid
                };

                var queueItem = JsonConvert.SerializeObject(sendGridSyncInstruction);

                await queue.AddAsync(queueItem);
            }

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("Events have been successfully queued for synchronisation.")
            };

            return response;
        }
    }
}