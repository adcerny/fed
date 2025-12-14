using Fed.Api.External.MicrosoftTeams;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
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
    public static class BakeryMinimumOrderPastryFunction
    {
        private const string FuncName = "BakeryMinimumOrderPastryFunction";
        private const string Schedule = "0 59 13 * * 1-5";
        private const string OrderName = "Bakery one off pastry order to reach minimum order quantities";

        private const int MinQuantity = 1;

        [FunctionName(FuncName)]
        public static Task RunNoMail(
            [TimerTrigger(Schedule)]TimerInfo timerInfo,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, TopUpBakeryPastryOrder);

        public async static Task TopUpBakeryPastryOrder(ServicesBag bag)
        {
            Guid fedBufferCustomerId = Guid.Parse(bag.Config.FedBufferCustomerId);
            Guid PastryCategoryId = Guid.Parse(bag.Config.PastryCategoryId);

            var fedClient = bag.FedClient;
            var logger = bag.Logger;

            //var nextWorkingDay = DateTime.Now.GetNextWorkingDay();
            var nextWorkingDay = DateTime.Now.GetNextWorkingDay();
            var nextOrderDate = Date.Create(nextWorkingDay);

            logger.LogInformation($"Delivery Date: {nextOrderDate}");
            logger.LogInformation($"Calling the Supplier Forecast to check how many bakery orders we have for the given delivery date...");

            var bakeryForecast = await bag.FedClient.GetSupplierForecastAsync((int)Suppliers.SevenSeeded, nextOrderDate);
            var fedCustomer = await bag.FedClient.GetCustomerAsync(fedBufferCustomerId);
            var fedBufferForecast = await bag.FedClient.GetForecastAsync(nextOrderDate, nextOrderDate, fedCustomer.PrimaryContact.Id);
            var pastryProducts = await bag.FedClient.GetProductsAsync(productCategoryId: PastryCategoryId);

            var nextBakeryOrder = bakeryForecast[nextOrderDate];
            var nextBufferOrder = fedBufferForecast.ContainsKey(nextOrderDate) ? fedBufferForecast[nextOrderDate].FirstOrDefault(o => o.Name == OrderName) : null;

            var additionalRequiredOrder = bag.MinimumOrderService.CalculateRequiredStockAsync(new List<int> { MinQuantity }, pastryProducts, nextBakeryOrder, nextBufferOrder);

            if (additionalRequiredOrder == null                      // If there are no additional items required to order
                || additionalRequiredOrder.Count == 0                // or the list of items is empty
                || !additionalRequiredOrder.Any(i => i.Item2 != 0))   // or the list of items doesn't have a single item with quantity > 0
            {
                logger.LogInformation("No additional items needed for pastry order.");

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
                        "Seven Seeded minimum pastry order reached through customer orders",
                        $"Customer orders suffice to reach the baker's minimum order pastry quantities!",
                        $"Hurray! We've got enough customer orders to reach the Seven Seeded minimum order quantities."));

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
                nextBufferOrder = await CreateNewOrder(fedClient, logger, nextOrderDate, timeslots, fedBufferAccount, orderItems);

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
                "Seven Seeded Pastry Buffer Stock Added",
                $"{orderItems.Count} bakery item/items have been ordered by {fedBufferAccount.CompanyName}",
                $"In order to reach the minimum pastry quantities we had to order additional items.",
                sections: new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Additional Products Ordered", $"An additional one-off pastry order has been placed by {fedBufferAccount.CompanyName} for {nextOrderDate.Value.ToString("dddd, dd/MM/yyyy")} in order to reach the minimum order quantities for Seven Seeded. In total we added an extra {sb.ToString()} to the final order.")
                });

            await bag.FedBot.SendMessage(teamsCard);

            logger.LogInformation($"Minimum Pastry Order TopUp has finished successfully.");
        }

        private static async Task UpdateExistingOrder(Web.Service.Client.FedWebClient fedClient, ILogger logger, List<RecurringOrderItem> orderItems, RecurringOrder oneOffOrder)
        {
            logger.LogInformation($"Updating one off order for the extra items...");
            oneOffOrder.OrderItems = orderItems;
            logger.LogInformation($"Sending updated one off order to web service...");
            await fedClient.UpdateRecurringOrderAsync(oneOffOrder.Id, oneOffOrder);
        }

        private static async Task<RecurringOrder> CreateNewOrder(Web.Service.Client.FedWebClient fedClient, ILogger logger, Date nextOrderDate, IList<Timeslot> timeslots, Customer fedBufferAccount, List<RecurringOrderItem> orderItems)
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
                    OrderName,
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