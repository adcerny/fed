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
    public static class BakeryMinimumOrderFunction
    {
        private const string FuncName = "BakeryMinimumOrderFunction";
        private const string Schedule = "0 59 13 * * 1-5";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timerInfo,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, TopUpBakeryOrder);

        // ToDo: Temp fix until we have the Supplier Price populated
        // and synced from Merchello. Need to ask Chris/Lex to do this!
        private static decimal GetSupplierPrice(string productSku)
        {
            switch (productSku)
            {
                // Seven Seeded Sourdough Loaf, Sliced - 800g 
                case "10676": return 3.36m;

                // Milk & Honey Sourdough Loaf, Sliced - 800g 
                case "10675": return 3.36m;

                // Hertfordshire Sourdough Loaf, Sliced - 800g 
                case "10677": return 2.86m;

                // Country Rye Loaf, Sliced - 800g 
                case "10678": return 2.58m;

                // Classic Viennoiserie
                case "10681": return 3.42m;

                // Almond Viennoiserie
                case "10682": return 5.76m;

                // Butter Croissant
                case "10683": return 3.2m;

                // Pain au Chocolat
                case "10684": return 3.64m;

                // Pain aux Raisins
                case "10685": return 4.88m;

                // Almond Croissant
                case "10686": return 5.56m;

                // Almond Pain au Chocolat
                case "10687": return 5.96m;

                // Unkown
                default: return 0;
            }
        }

        public async static Task TopUpBakeryOrder(ServicesBag bag)
        {
            var fedBufferCustomerId = Guid.Parse(bag.Config.FedBufferCustomerId);

            var logger = bag.Logger;

            var nextWorkingDay = DateTime.Now.GetNextWorkingDay();
            var nextOrderDate = Date.Create(nextWorkingDay);

            logger.LogInformation($"Delivery Date: {nextOrderDate}");
            logger.LogInformation($"Calling the Supplier Forecast to check how many bakery orders we have for the given delivery date...");

            var bakeryForecast = await bag.SuppliersService.GetSupplierForecastAsync((int)Suppliers.SevenSeeded, nextOrderDate, true);
            var nextBakeryOrder = bakeryForecast[nextOrderDate];

            var customerOrderValue =
                nextBakeryOrder.Sum(x =>
                    x.SupplierQuantity
                    * bag.MinimumOrderService.GetSupplierPrice(x.SupplierSKU));

            var additionalRequiredOrder =
                await bag.MinimumOrderService.CalculateAdditionalRequiredBreadStockAsync(nextBakeryOrder);

            if (additionalRequiredOrder == null                      // If there are no additional items required to order
                || additionalRequiredOrder.Count == 0                // or the list of items is empty
                || !additionalRequiredOrder.Any(i => i.Item2 > 0))   // or the list of items doesn't have a single item with quantity > 0
            {
                logger.LogInformation("No additional items need to get ordered. Skipping the rest of the function...");

                await bag.FedBot.SendMessage(
                    TeamsCard.Create(
                        CardType.Order,
                        FuncName,
                        "Seven Seeded minimum order reached through customer orders",
                        $"Customer orders suffice to reach the baker's minimum order value!",
                        $"Hurray! We've got enough customer orders to reach the Seven Seeded minimum order value of £{bag.MinimumOrderService.MinimumOrderValue}.",
                        additionalFacts: new List<KeyValuePair<string, string>>
                        {
                            new KeyValuePair<string, string>("Minimum Order Value", $"£{bag.MinimumOrderService.MinimumOrderValue}"),
                            new KeyValuePair<string, string>("Customer Order Value", $"£{customerOrderValue}"),
                            new KeyValuePair<string, string>("Additional Orders by Fed", $"£0"),
                            new KeyValuePair<string, string>("Final Order Value", $"£{customerOrderValue}")
                        }));

                return;
            }

            var additionalOrderValue =
                additionalRequiredOrder.Sum(x =>
                    x.Item2 * bag.MinimumOrderService.GetSupplierPrice(x.Item1.SupplierSKU));

            var totalOrderValue = customerOrderValue + additionalOrderValue;

            logger.LogInformation($"We have {nextBakeryOrder.Count} bakery items currently ordered for a total value of £{customerOrderValue}.");

            logger.LogInformation($"In order to reach the minimum order value of at least £{bag.MinimumOrderService.MinimumOrderValue} an additional order has to be placed.");

            logger.LogInformation($"The system calculated an additional order for the Fed Buffer account for a value of £{additionalOrderValue}.");

            var fedClient = bag.FedClient;

            // Load data from service
            logger.LogInformation($"Calling web service to get latest timeslot info, the fed buffer account and product details...");

            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var fedBufferAccount = await fedClient.GetCustomerAsync(fedBufferCustomerId);

            // Calculate how many extra bakery items we need to order
            var orderItems = new List<RecurringOrderItem>();

            foreach (var (product, quantity) in additionalRequiredOrder)
                orderItems.Add(new RecurringOrderItem { ProductId = product.Id, Quantity = quantity });

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
                    "Bakery one off order to reach minimum order value",
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

            // Send request to create recurring Order
            logger.LogInformation($"Sending one off order to web service...");
            await fedClient.CreateRecurringOrderAsync(oneOffOrder);

            // Notify in Teams
            logger.LogInformation($"Sending Teams notification...");

            var sb = new StringBuilder();

            foreach (var orderItem in oneOffOrder.OrderItems)
            {
                if (sb.Length > 0)
                    sb.Append(", ");

                var extraProduct = additionalRequiredOrder.Single(x => x.Item1.Id.Equals(orderItem.ProductId, StringComparison.OrdinalIgnoreCase)).Item1;
                sb.Append($"{orderItem.Quantity} of {extraProduct.ProductName} @ £{GetSupplierPrice(extraProduct.SupplierSKU)}");
            }

            var teamsCard = TeamsCard.Create(
                CardType.Order,
                FuncName,
                "Seven Seeded Buffer Stock Added",
                $"{orderItems.Count} bakery item/items have been ordered by {fedBufferAccount.CompanyName}",
                $"In order to reach the minimum order value of £{bag.MinimumOrderService.MinimumOrderValue} we had to order additional items.",
                additionalFacts: new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Minimum Order Value", $"£{bag.MinimumOrderService.MinimumOrderValue}"),
                    new KeyValuePair<string, string>("Customer Order Value", $"£{customerOrderValue}"),
                    new KeyValuePair<string, string>("Additional Orders by Fed", $"£{additionalOrderValue}"),
                    new KeyValuePair<string, string>("Final Order Value", $"£{totalOrderValue}")
                },
                sections: new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Additional Products Ordered", $"An additional one-off order has been placed by {fedBufferAccount.CompanyName} for {nextOrderDate.Value.ToString("dddd, dd/MM/yyyy")} in order to reach the minimum order value for Seven Seeded. In total we added an extra {sb.ToString()} to the final order.")
                });

            await bag.FedBot.SendMessage(teamsCard);

            logger.LogInformation($"Minimum Order TopUp has finished successfully.");
        }
    }
}