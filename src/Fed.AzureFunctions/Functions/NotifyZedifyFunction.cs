using Fed.Api.External.MicrosoftTeams;
using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class NotifyZedifyFunction
    {
        private const string FuncName = "NotifyZedifyFunction";

        [FunctionName(FuncName)]
        public static Task Run(
            [QueueTrigger(QueueNames.ZedifyNotification)]string queueItem,
            [Blob("zedify/daily-order.csv", FileAccess.Write)] Stream stream,
            ILogger logger)
            => FunctionRunner.RunWithArgAsync<(string, Stream)>(logger, FuncName, (queueItem, stream), ZedifyDeliveryNotification);

        public static async Task ZedifyDeliveryNotification((string, Stream) args, ServicesBag bag)
        {
            var (queueItem, csvStream) = args;
            var logger = bag.Logger;
            var fedClient = bag.FedClient;

            // 0. Validate Queue Item
            var isValidDate = DateTime.TryParse(queueItem, out DateTime orderDate);

            if (!isValidDate)
                throw new Exception($"Could not convert the queue item '{queueItem}' into a valid DateTime object.");

            // 1. Get Deliveries
            logger.LogInformation("Retrieving deliveries from Fed Web service...");
            var deliveries = await fedClient.GetDeliveriesAsync(Date.Create(orderDate));

            // 2. Generate CSV
            logger.LogInformation("Generating CSV for Zedify...");
            var csv = new StringBuilder();

            csv.AppendLine($"\"Consignment ID\",\"Reference\",\"Collection required?\",\"Collection time\",\"Collection Address Line 1\",\"Collection Address Line 2\",\"Collection Address Line 3\",\"Collection City\",\"Collection Postcode\",\"Delivery Instruction\",\"Recipient Name\",\"Recipient Telephone\",\"Delivery Address Line 1\",\"Delivery Address Line 2\",\"Delivery Address Line 3\",\"Delivery City\",\"Delivery Postcode\",\"Delivery time\",\"onforward\",\"Total Weight\",\"Delivery Date 1\",\"Delivery Quantity 1\",\"Price 1\"");

            foreach (var delivery in deliveries)
            {
                var deliveryPostcode = Address.NormalisePostcode(delivery.DeliveryPostcode);

                var deliveryInstructions =
                    delivery.DeliveryInstructions?.Substring(
                        0, Math.Min(500, delivery.DeliveryInstructions.Length));

                var isNewCustomer = delivery.Orders.Any(o => o.IsFirstOrder);
                var companyName = isNewCustomer ? $"NEW {delivery.DeliveryCompanyName}" : delivery.DeliveryCompanyName;

                csv.AppendLine($"\"{delivery.ShortId}\",\"{companyName}\",\"No\",,,,,,,\"{deliveryInstructions}\",\"{delivery.DeliveryFullName}\",,\"{delivery.DeliveryCompanyName}\",\"{delivery.DeliveryAddressLine1}\",\"{delivery.DeliveryAddressLine2}\",\"{delivery.DeliveryTown}\",\"{deliveryPostcode}\",\"{delivery.LatestTime}\",No,1,\"{delivery.DeliveryDate.Value.ToString("dd/MM/yyyy")}\",1,1");
            }

            // 3. Save CSV in Azure Blob Storage
            logger.LogInformation("Uploading CSV to Azure Blob Storage...");
            var csvContents = Encoding.UTF8.GetBytes(csv.ToString());
            var csvContentsWithUtf8Bom = Encoding.UTF8.GetPreamble().Concat(csvContents).ToArray();

            await csvStream.WriteAsync(csvContentsWithUtf8Bom, 0, csvContentsWithUtf8Bom.Length);

            // 4. Send Email
            var hasNoDeliveries = deliveries == null || deliveries.Count == 0;

            var html = hasNoDeliveries
                ? await EmailTemplates.ApplyZedifyNoOrdersTemplateAsync(orderDate)
                : await EmailTemplates.ApplyZedifyTemplateAsync(orderDate, bag.Config.ZedifyCsvUrl);

            var title = hasNoDeliveries
                ? $"No Confirmed Deliveries for {orderDate.ToString("dddd, dd MMMM yyyy")}"
                : $"Confirmed Deliveries for {orderDate.ToString("dddd, dd MMMM yyyy")}";

            var email = new Email
            {
                FromAddress = "noreply@fedteam.co.uk",
                ToAddresses = bag.Config.ZedifyEmailAddresses,
                BCCs = bag.Config.FedOpsEmailAddresses,
                Subject = title,
                PlainText = "",
                HtmlText = html.ToString()
            };

            var emailSent = await bag.SendGridService.SendMessageAsync(email);

            // 5. Notify in Teams
            if (emailSent)
            {
                await bag.FedBot.SendMessage(
                    TeamsCard.Create(
                        CardType.Delivery,
                        FuncName,
                        $"Zedify has been notified for the delivery on {orderDate.ToString("dddd, dd MMM yyyy")}.",
                        $"A CSV with all the drops and delivery details has been successfully generated for Zedify.",
                        $"Zedify can download the CSV from the link provided below.",
                        new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("Delivery Date", orderDate.ToString("dddd, dd MMMM yyyy")),
                            new KeyValuePair<string, string>("Total Drops", deliveries.Count.ToString()),
                            new KeyValuePair<string, string>("7AM-9AM Drops", deliveries?.Where(d => d.LatestTime.Hours.Equals(9))?.Count().ToString()),
                            new KeyValuePair<string, string>("9AM-12PM Drops", deliveries?.Where(d => d.LatestTime.Hours.Equals(12))?.Count().ToString())
                        },
                        null,
                        new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("Download Zedify CSV", bag.Config.ZedifyCsvUrl)
                        }));
            }
        }
    }
}