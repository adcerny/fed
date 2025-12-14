using Fed.Api.External.MicrosoftTeams;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class MinimumOrderSupplierFunction
    {
        private const string FuncName = "SupplierMinimumOrderFunction";
        private const string OrderName = "One off order to reach minimum order quantities for {0}";

        [FunctionName(FuncName)]
        public static Task RunNoMail(
            [QueueTrigger(QueueNames.SupplierMinimumOrder)]string queueItem,
            ILogger logger)
            => FunctionRunner.RunWithArgAsync(logger, FuncName, queueItem, TopUpBakerySupplierOrder);

        public async static Task TopUpBakerySupplierOrder(string queueItem, ServicesBag bag)
        {

            var request = JsonConvert.DeserializeObject<SupplierTopUpRequest>(queueItem);

            Guid fedBufferCustomerId = Guid.Parse(bag.Config.FedBufferCustomerId);

            var fedClient = bag.FedClient;
            var logger = bag.Logger;

            //var nextWorkingDay = DateTime.Now.GetNextWorkingDay();
            var nextWorkingDay = DateTime.Now.GetNextWorkingDay();
            var nextOrderDate = Date.Create(nextWorkingDay);

            logger.LogInformation($"Delivery Date: {nextOrderDate}");

            var supplier = await fedClient.GetSupplierAsync(request.SupplierId);

            var orderName = String.Format(OrderName, supplier.Name);

            logger.LogInformation($"Calling the Supplier Forecast for {supplier.Name} to check how many orders we have for the given delivery date...");

            var supplierForecast = await fedClient.GetSupplierForecastAsync(request.SupplierId, nextOrderDate);
            var fedCustomer = await fedClient.GetCustomerAsync(fedBufferCustomerId);
            var fedBufferForecast = await fedClient.GetForecastAsync(nextOrderDate, nextOrderDate, fedCustomer.PrimaryContact.Id);
            var allProducts = await bag.FedClient.GetProductsAsync();
            var products = allProducts.Where(p => p.SupplierId == request.SupplierId.ToString() &&
                                        (!request.ProductCategoryId.HasValue || p.ProductCategoryId == request.ProductCategoryId)).ToList();

            var nextSupplierOrder = supplierForecast[nextOrderDate];
            var nextBufferOrder = fedBufferForecast.ContainsKey(nextOrderDate) ? fedBufferForecast[nextOrderDate].FirstOrDefault(o => o.Name == orderName) : null;

            var additionalRequiredOrder = bag.MinimumOrderService.CalculateRequiredStockAsync(request.MinQuantities, products, nextSupplierOrder, nextBufferOrder);

            if (additionalRequiredOrder == null                      // If there are no additional items required to order
                || additionalRequiredOrder.Count == 0                // or the list of items is empty
                || !additionalRequiredOrder.Any(i => i.Item2 != 0))   // or the list of items doesn't have a single item with quantity > 0
            {
                logger.LogInformation("No additional items needed for order.");

                if (nextBufferOrder != null)
                {
                    logger.LogInformation($"Deleting exiting buffer order {nextBufferOrder.Id}");
                    await bag.FedClient.DeleteRecurringOrderAsync(nextBufferOrder.Id);
                }

                logger.LogInformation("Skipping the rest of the function");

                await bag.FedBot.SendMessage(
                    TeamsCard.Create(
                        CardType.Order,
                        FuncName,
                        $"{supplier.Name} minimum order reached through customer orders",
                        $"Customer orders suffice to reach {supplier.Name} minimum order quantities!",
                        $"Hurray! We've got enough customer orders to reach the {supplier.Name} minimum order quantities."));

                return;
            }

            //// Load data from service
            logger.LogInformation($"Calling web service to get latest timeslot info, the fed buffer account and product details...");

            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var fedBufferAccount = await fedClient.GetCustomerAsync(fedBufferCustomerId);

            var orderItems = new List<RecurringOrderItem>();

            foreach (var (product, quantity) in additionalRequiredOrder.Where(i => i.Item2 > 0))
                orderItems.Add(new RecurringOrderItem { ProductId = product.Id, Quantity = quantity });

            if (nextBufferOrder != null)
                await UpdateExistingOrder(fedClient, logger, orderItems, nextBufferOrder);
            else
                nextBufferOrder = await CreateNewOrder(fedClient, logger, nextOrderDate, timeslots, fedBufferAccount, orderItems, orderName);

            //// Notify in Teams
            logger.LogInformation($"Sending Teams notification...");

            var sb = new StringBuilder();

            foreach (var orderItem in nextBufferOrder.OrderItems)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                var extraProduct = additionalRequiredOrder.Single(x => x.Item1.Id.Equals(orderItem.ProductId, StringComparison.OrdinalIgnoreCase)).Item1;
                sb.Append($"{orderItem.Quantity} of {extraProduct.ProductName}");
            }

            var teamsCard = TeamsCard.Create(
                CardType.Order,
                FuncName,
                $"{supplier.Name} Buffer Stock Added",
                $"{orderItems.Count} yummy tummy item{(orderItems.Count > 1 ? "s" : "")} have been ordered by {fedBufferAccount.CompanyName}",
                $"In order to reach the minimum order quantities for {supplier.Name} we had to order additional items.",
                sections: new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Additional Products Ordered", $"An additional one-off order has been placed by {fedBufferAccount.CompanyName} for {nextOrderDate.Value.ToString("dddd, dd/MM/yyyy")} in order to reach the minimum order quantities for {supplier.Name}. In total we added an extra {sb.ToString()} to the final order.")
                });

            await bag.FedBot.SendMessage(teamsCard);

            logger.LogInformation($"{supplier.Name} Order TopUp has finished successfully.");
        }

        private static async Task UpdateExistingOrder(Web.Service.Client.FedWebClient fedClient, ILogger logger, List<RecurringOrderItem> orderItems, RecurringOrder oneOffOrder)
        {
            logger.LogInformation($"Updating one off order for the extra items...");
            oneOffOrder.OrderItems = orderItems;
            logger.LogInformation($"Sending updated one off order to web service...");
            await fedClient.UpdateRecurringOrderAsync(oneOffOrder.Id, oneOffOrder);
        }

        private static async Task<RecurringOrder> CreateNewOrder(Web.Service.Client.FedWebClient fedClient, ILogger logger, Date nextOrderDate, IList<Timeslot> timeslots, Customer fedBufferAccount, List<RecurringOrderItem> orderItems, string orderName)
        {
            logger.LogInformation($"Creating a one off order for the extra items...");

            // Create new order
            var contact = fedBufferAccount.Contacts[0];
            var deliveryAddress = contact.GetPrimaryDeliveryAddress();
            var billingAddress = contact.GetPrimaryBillingAddress();
            var timeslot =
                timeslots
                    .Where(t => t.DayOfWeek == nextOrderDate.Value.DayOfWeek && t.AvailableCapacity > 0)
                    .First();

            var oneOffOrder =
                new RecurringOrder(
                    Guid.Empty,
                    orderName,
                    contact.Id,
                    deliveryAddress.Id,
                    billingAddress.Id,
                    nextOrderDate.Value,
                    nextOrderDate.Value,
                    WeeklyRecurrence.OneOff,
                    timeslot.Id)
                {
                    OrderItems = orderItems
                };

            //// Send request to create recurring Order
            logger.LogInformation($"Sending one off order to web service...");
            await fedClient.CreateRecurringOrderAsync(oneOffOrder);
            return oneOffOrder;
        }
    }
}