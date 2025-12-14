using Fed.Api.External.MicrosoftTeams;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class DailyOrderDeadlineFunction
    {
        private const string FuncName = "DailyOrderDeadlineFunction";
        private const string Schedule = "0 1 16 * * 1-5";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timer,
            [Queue(QueueNames.PaymentProcessing)] IAsyncCollector<string> paymentsQueue,
            [Queue(QueueNames.ZedifyNotification)] IAsyncCollector<string> zedifyQueue,
            [Queue(QueueNames.SupplierFinalOrder)] IAsyncCollector<string> supplierQueue,
            ILogger logger) =>
            FunctionRunner.RunWithArgAsync(logger, FuncName, (paymentsQueue, zedifyQueue, supplierQueue), PlaceOrders);

        public static async Task PlaceOrders(
            (IAsyncCollector<string>,
            IAsyncCollector<string>,
            IAsyncCollector<string> notifyZedifyQueue) queues,
            ServicesBag bag)
        {
            // Init:
            var (paymentProcessingQueue, supplierFinalOrderQueue, notifyZedifyQueue) = queues;
            var logger = bag.Logger;
            var fedBot = bag.FedBot;
            var fedClient = bag.FedClient;

            // Place Orders:
            var now = DateTime.Now.ToBritishTime();
            var nextWorkingDay = now.GetNextWorkingDay();
            var nextOrderDate = Date.Create(nextWorkingDay);

            logger.LogInformation($"Sending HTTP request to the Fed Web Service. Placing orders for {nextOrderDate}.");
            var generatedOrders = await bag.FedClient.PlaceOrdersAsync(nextOrderDate);

            // Group Orders into Deliveries:
            logger.LogInformation($"Grouping orders into deliveries...");
            var generatedDeliveries = await bag.FedClient.CreateDeliveriesAsync(nextOrderDate);

            // Trigger Payments, Zedify Email and Supplier Final Order Call:
            var ordersExist = generatedOrders != null && generatedOrders.Count > 0;

            if (ordersExist)
            {
                var queueItem = nextOrderDate.Value.ToString("yyyy-MM-dd");

                await paymentProcessingQueue.AddAsync(queueItem);
                await supplierFinalOrderQueue.AddAsync(queueItem);
                await notifyZedifyQueue.AddAsync(queueItem);

                logger.LogInformation($"Placed the item '{queueItem}' into the queues '{QueueNames.PaymentProcessing}', '{QueueNames.SupplierFinalOrder}', '{QueueNames.ZedifyNotification}'.");
            }

            // Teams Notification:
            var facts =
                new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("Delivery Date", nextOrderDate.Value.ToString("dddd, dd MMM yyyy")),
                        new KeyValuePair<string, string>("Total Orders", ordersExist ? generatedOrders.Count.ToString() : "0"),
                        new KeyValuePair<string, string>("Total Deliveries", ordersExist ? generatedDeliveries.Count.ToString() : "0")
                    };

            if (!ordersExist)
            {
                logger.LogInformation($"No orders have been placed for {nextOrderDate}.");

                await bag.FedBot.SendMessage(
                    TeamsCard.Create(
                        CardType.Order,
                        FuncName,
                        $"No Orders have been placed for {nextOrderDate.Value.ToString("dddd, dd MMM yyyy")}.",
                        $"The daily deadline process has successfully finished without placing any orders for the next working day.",
                        $"No orders have been placed, because no recurring order or one-off order is scheduled for {nextOrderDate}.",
                        facts, null, null));

                return;
            }

            logger.LogInformation($"{generatedOrders.Count} orders have been generated:");

            var generatedOrderIds = new StringBuilder();

            foreach (var order in generatedOrders)
            {
                generatedOrderIds.Append($", {order.ShortOrderId}");

                logger.LogInformation($"ORDER ID: {order.GeneratedOrderId}, RECURRING ORDER ID: {order.RecurringOrderId}");
            }

            logger.LogInformation($"{generatedDeliveries.Count} deliveries have been generated:");

            var generatedDeliveryIds = new StringBuilder();

            foreach (var delivery in generatedDeliveries)
            {
                var orderIds = new StringBuilder();

                foreach (var order in delivery.Orders)
                {
                    if (orderIds.Length > 0)
                        orderIds.Append(", ");

                    orderIds.Append(order.ShortId);
                }

                generatedDeliveryIds.Append($", {delivery.ShortId}");

                logger.LogInformation($"DELIVERY ID: {delivery.ShortId}, ORDER IDs: {orderIds}");
            }

            var title =
                generatedDeliveries.Count == 1
                ? $"1 Delivery has been placed for {nextOrderDate.Value.ToString("dddd, dd MMM yyyy")}."
                : $"{generatedDeliveries.Count} Deliveries have been placed for {nextOrderDate.Value.ToString("dddd, dd MMM yyyy")}.";

            await bag.FedBot.SendMessage(
                TeamsCard.Create(
                    CardType.Order,
                    FuncName,
                    title,
                    $"The daily deadline process has successfully created {generatedOrders.Count} order(s) for the next working day.",
                    $"The {generatedOrders.Count} order(s) have been grouped into {generatedDeliveries.Count} deliveries.",
                    facts,
                    new List<KeyValuePair<string, string>>
                    {
                            new KeyValuePair<string, string>("Generated Delivery IDs:", generatedDeliveryIds.ToString().Substring(2, generatedDeliveryIds.Length - 2)),
                            new KeyValuePair<string, string>("Generated Order IDs:", generatedOrderIds.ToString().Substring(2, generatedOrderIds.Length - 2))
                    },
                    new List<KeyValuePair<string, string>>
                    {
                            new KeyValuePair<string, string>("View in Fed Portal", $"{bag.Config.FedPortalUrl}/deliveries?deliveryDate={nextOrderDate}"),
                            new KeyValuePair<string, string>("View Pick Sheets", $"{bag.Config.FedPortalUrl}/deliveries/pickSheets?deliveryDate={nextOrderDate}&sortBy=Slot"),
                            new KeyValuePair<string, string>("Print Labels", $"{bag.Config.FedPortalUrl}/deliveries/labels?deliveryDate={nextOrderDate}")
                    }));
        }
    }
}