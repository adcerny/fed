using Fed.Api.External.MicrosoftTeams;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Infrastructure.Factories;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class PaymentProcessingFunction
    {
        private const string FuncName = "PaymentProcessingFunction";

        [FunctionName(FuncName)]
        [Singleton]
        public static Task Run(
            [QueueTrigger(QueueNames.PaymentProcessing)]string queueItem,
            ILogger logger)
            => FunctionRunner.RunWithArgAsync(
                logger, FuncName, queueItem, ProcessPayments);

        public static async Task ProcessPayments(string queueItem, ServicesBag bag)
        {
            var logger = bag.Logger;
            var fedBot = bag.FedBot;

            // 1. Validate QueueItem

            var isValidDate = DateTime.TryParse(queueItem, out DateTime paymentDate);
            if (!isValidDate)
                throw new Exception($"Could not convert the queue item '{queueItem}' into a valid DateTime object.");

            // 2. Process payments:

            var paymentService =
                PaymentProcessingFactory.CreateProcessPaymentsService(
                    bag.Config.Braintree.EnvironmentName,
                    bag.Config.Braintree.MerchantId,
                    bag.Config.Braintree.MerchantAccountId,
                    bag.Config.Braintree.PublicKey,
                    bag.Config.Braintree.PrivateKey,
                    bag.SqlConfig.ConnectionString,
                    bag.Config.PaymentMaxConcurrentOps,
                    logger);

            logger.LogInformation($"Calling the payment service for date '{paymentDate}'...");

            CardTransactionBatch batch = null;

            try
            {
                batch = await paymentService.ProcessPayments(paymentDate);
            }
            catch (Exception ex)
            {
                logger.LogError($"Payment processing failed with message: {ex.Message}.");

                await fedBot.SendMessage(
                    TeamsCard.CreateWarning(
                        ex,
                        FuncName,
                        "ATTENTION: Could not successfully process payments"));

                return;
            }

            // 3. Notify in Teams:

            var totalTransactions = batch?.CardTransactions?.Count ?? 0;
            var unsuccessfulTransactions = batch?.CardTransactions.Where(t => t.Status != CardTransactionStatus.Paid && t.Status != CardTransactionStatus.Refunded).OrEmptyIfNull().ToList();


            string title = $"Payments have been processed for {paymentDate.ToString("dddd, dd MMM yyyy")}.";
            string subtitle = totalTransactions > 0 ? $"{totalTransactions - unsuccessfulTransactions?.Count() ?? 0} out of {totalTransactions} payments were successful." : "No payments were required.";

            StringBuilder summary = new StringBuilder();
            if (unsuccessfulTransactions?.Count() > 0)
            {
                summary.Append("Failed transactions: <ul>");
                foreach (var transaction in unsuccessfulTransactions.OrEmptyIfNull())
                {
                    var delivery = await bag.FedClient.GetDeliveryAsync(transaction.DeliveryId.ToString());
                    string responseText = string.IsNullOrEmpty(transaction.ResponseCode) ?
                        $"'{transaction.ErrorMessage}'" :
                        $"<a href=https://developers.braintreepayments.com/reference/general/processor-responses/authorization-responses#code-{transaction.ResponseCode}>{transaction.ResponseText}</a>";
                    summary.Append($"<li>{delivery.DeliveryCompanyName} payment of {transaction.AmountRequested.ToString("C", new CultureInfo("en-GB"))} for delivery on {delivery.DeliveryDate.Value.ToString("dddd, MMM dd")} failed. Response was {responseText}.</li>");
                }
                summary.Append("</ul>");
            }

            logger.LogInformation($"{title} {subtitle} ");

            await fedBot.SendMessage(
               TeamsCard.Create(
                    CardType.Payment,
                    FuncName,
                    title,
                    subtitle,
                    summary.Length > 0 ? summary.ToString() : null,
                    new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("Order date", paymentDate.ToString("dddd, dd MMM yyyy"))
                    },
                    null,
                    null
                ));
        }
    }
}