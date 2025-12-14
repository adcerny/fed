using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class ProcessPaymentsService : IProcessPaymentsService
    {
        IPaymentGatewayService _paymentGatewayService;
        IPaymentsHandler _paymentsHandler;
        IDeliveriesHandler _deliveriesHandler;
        IContactsHandler _contactsHandler;
        ILogger _logger;
        private int _maxConcurrentOperations;

        public ProcessPaymentsService(
            IPaymentGatewayService paymentGatewayService,
            IPaymentsHandler paymentsHandler,
            IDeliveriesHandler deliveriesHandler,
            IContactsHandler contactsHandler,
            ILogger logger,
            int maxConcurrentOperations)
        {
            _paymentGatewayService = paymentGatewayService;
            _paymentsHandler = paymentsHandler;
            _deliveriesHandler = deliveriesHandler;
            _contactsHandler = contactsHandler;
            _logger = logger;
            _maxConcurrentOperations = maxConcurrentOperations;
        }

        public async Task<CardTransactionBatch> ProcessPayments(Date deliveryDate)
        {
            var batchId = Guid.NewGuid();
            _logger.LogInformation($"Starting payments for {deliveryDate}. Batch Id is {batchId}");

            List<PaymentRequest> paymentRequests = new List<PaymentRequest>();
            paymentRequests.AddRange(await GetPaymentRequests(deliveryDate, batchId));
            paymentRequests.AddRange(await GetRefundRequests(deliveryDate.AddDays(-1), batchId));

            //If no payments needed, return null
            if (paymentRequests?.Count == 0)
            {
                _logger.LogInformation($"No payments needed for {deliveryDate}, batch Id {batchId}");
                return null;
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var batch = new CardTransactionBatch(batchId, deliveryDate, DateTime.Now.ToBritishTime(), null, null);
            await _paymentsHandler.ExecuteAsync(new CreateCommand<CardTransactionBatch>(batch));

            _logger.LogInformation($"Starting payment processing for {deliveryDate}, batch Id {batchId}");
            await paymentRequests.ForEachAsync(_maxConcurrentOperations, _paymentGatewayService.ProcessPayment);

            batch.TimeEnded = DateTime.Now.ToBritishTime();

            stopwatch.Stop();
            _logger.LogInformation($"Finished payment processing for {deliveryDate}, batch Id {batchId}. Time elapsed was {(stopwatch.Elapsed.ToString("mm\\:ss\\.ff"))}");

            await _paymentsHandler.ExecuteAsync(new UpdateCommand<CardTransactionBatch>(batch.Id, batch));

            //refresh batch from repository so that we have an accurate picture of what we have recorded
            var processedBatch = await _paymentsHandler.ExecuteAsync(new GetByIdQuery<CardTransactionBatch>(batch.Id));

            return processedBatch;
        }

        public async Task<bool> UpdateCardTransactions(Guid batchId)
        {
            var transactions = await _paymentsHandler.ExecuteAsync(new GetCardTransactionsQuery(null, CardTransactionStatus.Requested));

            await transactions.ForEachAsync(_maxConcurrentOperations, _paymentGatewayService.UpdateCardTransaction);

            return true;
        }

        private async Task<IList<PaymentRequest>> GetPaymentRequests(Date deliveryDate, Guid batchId)
        {

            List<PaymentRequest> paymentRequests = new List<PaymentRequest>();
            var deliveryIds = await _paymentsHandler.ExecuteAsync(new GetDeliveryIdsForPaymentQuery(deliveryDate));

            //get payments
            foreach (Guid id in deliveryIds)
            {
                var delivery = await _deliveriesHandler.ExecuteAsync(new GetByIdQuery<Delivery>(id));
                var total = delivery.GetDeliveryTotal();
                if (total <= 0)
                    continue;

                var contact = await _contactsHandler.ExecuteAsync(new GetByIdQuery<Contact>(delivery.ContactId));

                if (contact.CardTokens == null || contact.CardTokens.Count == 0)
                {
                    _logger.LogWarning($"Contact {contact.ShortId} has no card token and will not be processed.");
                    continue;
                }

                paymentRequests.Add(new PaymentRequest
                {
                    DeliveryId = id,
                    Amount = total,
                    CardTransactionBatchId = batchId,
                    CardTokenId = contact.CardTokens.Where(c => c.IsPrimary).First().Id,
                    Status = CardTransactionStatus.Requested
                });
            }
            return paymentRequests;
        }

        private async Task<IList<PaymentRequest>> GetRefundRequests(Date deliveryDate, Guid batchId)
        {
            List<PaymentRequest> paymentRequests = new List<PaymentRequest>();
            var refunds = await _paymentsHandler.ExecuteAsync(new GetDeliveryAndTrasactionIdsForRefundQuery(deliveryDate));

            foreach ((Guid, Guid) refund in refunds)
            {
                var delivery = await _deliveriesHandler.ExecuteAsync(new GetByIdQuery<Delivery>(refund.Item1));
                decimal refundAmount = GetDeliveryRefundAmount(delivery);

                //if we owe money, add the refund to the list of payment requests
                if (refundAmount > 0)
                {
                    var contact = await _contactsHandler.ExecuteAsync(new GetByIdQuery<Contact>(delivery.ContactId));
                    var cardTokenId = contact.CardTokens.Where(c => c.IsPrimary).First();

                    paymentRequests.Add(new PaymentRequest
                    {
                        DeliveryId = refund.Item1,
                        OriginalCardTransactionId = refund.Item2,
                        Amount = -refundAmount,
                        CardTransactionBatchId = batchId,
                        CardTokenId = contact.CardTokens.Where(c => c.IsPrimary).First().Id,
                        Status = CardTransactionStatus.Requested
                    });
                }
            }
            return paymentRequests;
        }

        private static decimal GetDeliveryRefundAmount(Delivery delivery)
        {
            decimal refundAmount = 0;

            //Add up the value of all shortages taking into count the value of any replacements
            foreach (var shortage in delivery.DeliveryShortages.OrEmptyIfNull())
            {
                var shortedItemPrice = delivery.Orders
                                           .Where(o => o.Id == shortage.OrderId)
                                           .Single().OrderItems.Where(i => i.ProductId == shortage.ProductId)
                                           .First().RefundablePrice;

                decimal shortedValue = shortedItemPrice * (shortage.DesiredQuantity - shortage.ActualQuantity);
                decimal replacementsValue = shortage.Replacements?.Sum(r => r.ProductPrice * r.Quantity) ?? 0;

                if (replacementsValue < shortedValue)
                    refundAmount += shortedValue - replacementsValue;
            }

            return refundAmount;
        }
    }
}
