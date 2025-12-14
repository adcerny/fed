using Fed.Api.External.MicrosoftTeams;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.ValueTypes;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class ShortagesNotificationFunction
    {
        private const string FuncName = "ShortagesNotificationFunction";
        private const string Schedule = "0 30 7,8,9,10,11,12,13,14,15 * * 1-5";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timerInfo,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, ReportShortagesAsync);

        public static async Task ReportShortagesAsync(ServicesBag bag)
        {
            const string webhookUrl = "https://outlook.office.com/webhook/4cc77159-f19d-4930-8b9c-7e93d3736bb2@a64e24f4-52b9-4b42-80ae-bd5c1085942c/IncomingWebhook/120394eefe624568a76d86abbaf1b285/b3ee8686-9c9e-4c20-a093-120e5234b637";
            const int messageLimit = 30;

            var sinceTime = new TimeSpan(DateTime.Now.Hour - 1, 30, 0);

            var logger = bag.Logger;
            logger.LogInformation("Retrieving the most recent shortages recorded for the day...");

            var deliveryShortages = await bag.FedClient.GetDeliveryShortagesAsync(Date.Today);
            var deliveryAdditions = await bag.FedClient.GetDeliveryAdditionsAsync(Date.Today);

            if ((deliveryShortages == null || deliveryShortages.Count == 0)
                && (deliveryAdditions == null || deliveryAdditions.Count == 0))
            {
                logger.LogInformation("No shortages or additions reported for today (yet).");
                return;
            }

            logger.LogInformation($"{deliveryShortages.Count} shortages have been recorded so far for today.");
            logger.LogInformation($"{deliveryAdditions.Count} additions have been recorded so far for today.");

            foreach (var shortage in deliveryShortages.Take(messageLimit))
            {
                logger.LogInformation($"{shortage.ProductName} has been shortened from {shortage.DesiredQuantity} to {shortage.ActualQuantity}.");
            }

            foreach (var addition in deliveryAdditions.Take(messageLimit))
            {
                logger.LogInformation($"{addition.Quantity} x {addition.ProductName} has been added to an order today.");
            }

            logger.LogInformation("Loading all deliveries for today...");

            var deliveries = await bag.FedClient.GetDeliveriesAsync(Date.Today);


            var report = new List<KeyValuePair<string, string>>();

            foreach (var delivery in deliveries.Where(d => d.HasShortagesSince(sinceTime) || d.HasAdditionsSince(sinceTime)))
            {
                var shortageCount = delivery.HasShortages() ? delivery.DeliveryShortages.Count : 0;
                var additionsCount = delivery.HasAdditions() ? delivery.DeliveryAdditions.Count : 0;
                var title = $"{delivery.DeliveryCompanyName} had {shortageCount} shortages and {additionsCount} additions in their delivery today.";

                var msg = new StringBuilder();

                if (delivery.HasShortages())
                {
                    msg.AppendLine("Shortages:");
                    foreach (var shortage in delivery.DeliveryShortages)
                    {
                        msg.AppendLine($"{shortage.ProductName} has been shortened from {shortage.DesiredQuantity} to {shortage.ActualQuantity}, because of {shortage.Reason}.");
                    }
                }
                else
                {
                    msg.AppendLine("Shortages: No shortages.");
                }

                if (delivery.HasAdditions())
                {
                    msg.AppendLine("Additions:");
                    foreach (var addition in delivery.DeliveryAdditions)
                    {
                        var str = string.IsNullOrEmpty(addition.Notes)
                            ? $"{addition.Quantity} x {addition.ProductName} has been added to the delivery."
                            : $"{addition.Quantity} x {addition.ProductName} has been added to the delivery ({addition.Notes}).";

                        msg.AppendLine(str);
                    }
                }
                else
                {
                    msg.AppendLine("Additions: None.");
                }

                report.Add(new KeyValuePair<string, string>(title, msg.ToString()));
            }

            var teamsCard =
                TeamsCard.Create(
                    CardType.Warning,
                    FuncName,
                    "Shortages have been recorded",
                    $"{report.Count} deliveries have been affected by shortages and/or substitutions today.",
                    $"Please click the link below to view more details.",
                    sections: report,
                    urlActions: new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("View Shortages", $"https://portal.fedteam.co.uk/reports/GetDeliveryShortageReportByDate?fromDate={Date.Today}&toDate={Date.Today}") });

            logger.LogInformation(teamsCard.AsJson());

            var shortagesReporter = FedBot.Create(bag.Logger, webhookUrl);

            logger.LogInformation("Sending insanity report to teams... need help from the devs, this ain't good, innit!");

            await shortagesReporter.SendMessage(teamsCard);
        }
    }
}