using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using Fed.Tests.Common;
using Fed.Web.Service.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fed.Tests.Integration
{
    [Collection("DeliveryTests")]
    public class SuppliersForecastTests
    {
        [Fact]
        public async Task SuppliersForecastTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());
            var randomStr = Guid.NewGuid().ToString("N").Substring(0, 4);
            var products = await fedClient.GetProductsAsync(null);
            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var customers = await fedClient.GetCustomersAsync(true);
            var contact = customers.First(c => c.Contacts[0].BillingAddresses != null && c.Contacts[0].DeliveryAddresses != null).Contacts[0];

            var deliveryDate = DateTime.Today.AddDays(1).GetNextWorkingDay();

            var startDate = deliveryDate;

            var nextDayTimeslot =
                timeslots
                    .Where(t => t.DayOfWeek == deliveryDate.DayOfWeek)
                    .First();

            var abelAndColeProducts = products.Where(p => Suppliers.AbelAndCole.MatchesSupplierId(p.SupplierId)).ToList();

            var product1 = abelAndColeProducts[0];
            var product2 = abelAndColeProducts[1];
            var product3 = abelAndColeProducts[2];

            // Delete any orders and deliveries for the random Date
            await fedClient.DeleteDeliveriesAsync(deliveryDate);

            // Get initial forecast
            var originalForecast = await fedClient.GetSupplierForecastAsync((int)Suppliers.AbelAndCole, deliveryDate);

            var originalProduct1Quantity =
                originalForecast == null || !originalForecast.ContainsKey(deliveryDate)
                ? 0
                : originalForecast[deliveryDate].SingleOrDefault(x => x.SupplierSKU.Equals(product1.SupplierSKU))?.SupplierQuantity ?? 0;

            var originalProduct2Quantity =
                originalForecast == null || !originalForecast.ContainsKey(deliveryDate)
                ? 0
                : originalForecast[deliveryDate].SingleOrDefault(x => x.SupplierSKU.Equals(product2.SupplierSKU))?.SupplierQuantity ?? 0;

            var originalProduct3Quantity = originalForecast == null || !originalForecast.ContainsKey(deliveryDate)
                ? 0
                : originalForecast[deliveryDate].SingleOrDefault(x => x.SupplierSKU.Equals(product3.SupplierSKU))?.SupplierQuantity ?? 0;

            // Generate two recurring ordres

            var recurringOrder1 =
                    new RecurringOrder(
                        Guid.NewGuid(),
                        $"Test-({randomStr})-1",
                        contact.Id,
                        contact.DeliveryAddresses.FirstOrDefault().Id,
                        contact.BillingAddresses.FirstOrDefault().Id,
                        startDate,
                        Date.MaxDate.Value,
                        WeeklyRecurrence.EveryWeek,
                        nextDayTimeslot.Id)
                    {
                        OrderItems = new List<RecurringOrderItem>
                            {
                            new RecurringOrderItem { ProductId = product1.Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = product2.Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = product3.Id, Quantity = 30 }
                            }
                    };

            var createdrecurringOrder1 = await fedClient.CreateRecurringOrderAsync(recurringOrder1);

            var recurringOrder2 =
                    new RecurringOrder(
                        Guid.NewGuid(),
                        $"Test-({randomStr})-2",
                        contact.Id,
                        contact.DeliveryAddresses.FirstOrDefault().Id,
                        contact.BillingAddresses.FirstOrDefault().Id,
                        startDate,
                        Date.MaxDate.Value,
                        WeeklyRecurrence.EveryWeek,
                        nextDayTimeslot.Id)
                    {
                        OrderItems = new List<RecurringOrderItem>
                            {
                            new RecurringOrderItem { ProductId = product1.Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = product2.Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = product3.Id, Quantity = 30 }
                            }
                    };

            var createdrecurringOrder2 = await fedClient.CreateRecurringOrderAsync(recurringOrder2);

            // Get a forecast for the next day

            var forecast = await fedClient.GetSupplierForecastAsync((int)Suppliers.AbelAndCole, deliveryDate);

            Assert.Equal(originalProduct1Quantity + 20, forecast[deliveryDate].Single(x => x.SupplierSKU.Equals(product1.SupplierSKU)).SupplierQuantity);
            Assert.Equal(originalProduct2Quantity + 40, forecast[deliveryDate].Single(x => x.SupplierSKU.Equals(product2.SupplierSKU)).SupplierQuantity);
            Assert.Equal(originalProduct3Quantity + 60, forecast[deliveryDate].Single(x => x.SupplierSKU.Equals(product3.SupplierSKU)).SupplierQuantity);

            // Generate orders

            await fedClient.PlaceOrdersAsync(deliveryDate);
            await fedClient.CreateDeliveriesAsync(deliveryDate);

            // Get a forecast for the next day

            forecast = await fedClient.GetSupplierForecastAsync((int)Suppliers.AbelAndCole, deliveryDate);

            Assert.Equal(originalProduct1Quantity + 20, forecast[deliveryDate].Single(x => x.SupplierSKU.Equals(product1.SupplierSKU)).SupplierQuantity);
            Assert.Equal(originalProduct2Quantity + 40, forecast[deliveryDate].Single(x => x.SupplierSKU.Equals(product2.SupplierSKU)).SupplierQuantity);
            Assert.Equal(originalProduct3Quantity + 60, forecast[deliveryDate].Single(x => x.SupplierSKU.Equals(product3.SupplierSKU)).SupplierQuantity);

            // Move one recurring order to a different date

            var differentTimeslot =
                timeslots
                    .Where(t => t.DayOfWeek != deliveryDate.DayOfWeek)
                    .First();

            createdrecurringOrder1.TimeslotId = differentTimeslot.Id;

            await fedClient.UpdateRecurringOrderAsync(createdrecurringOrder1.Id, createdrecurringOrder1);

            // Get forecast for the next day

            forecast = await fedClient.GetSupplierForecastAsync((int)Suppliers.AbelAndCole, deliveryDate);

            Assert.Equal(originalProduct1Quantity + 20, forecast[deliveryDate].Single(x => x.SupplierSKU.Equals(product1.SupplierSKU)).SupplierQuantity);
            Assert.Equal(originalProduct2Quantity + 40, forecast[deliveryDate].Single(x => x.SupplierSKU.Equals(product2.SupplierSKU)).SupplierQuantity);
            Assert.Equal(originalProduct3Quantity + 60, forecast[deliveryDate].Single(x => x.SupplierSKU.Equals(product3.SupplierSKU)).SupplierQuantity);

            //delete recurring orders to make test repeatable
            await fedClient.DeleteRecurringOrderAsync(createdrecurringOrder1.Id);
            await fedClient.DeleteRecurringOrderAsync(createdrecurringOrder2.Id);

            //tidy up
            await fedClient.DeleteDeliveriesAsync(deliveryDate);

        }
    }

    public class ForecastTests
    {
        [Fact]
        public async Task ForecastTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var products = await fedClient.GetProductsAsync(null);
            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var customers = await fedClient.GetCustomersAsync(true);

            var earlyMorningTimesltos =
                timeslots
                    .Where(t => t.EarliestTime == TimeSpan.FromHours(7))
                    .OrderBy(t => t.DayOfWeek)
                    .ToList();

            var contact = customers.First(c => c.Contacts[0].BillingAddresses != null && c.Contacts[0].DeliveryAddresses != null).Contacts[0];

            var randomStr = Guid.NewGuid().ToString("N").Substring(0, 4);
            var startDate = new DateTime(2030, 01, 01);

            // Create one recurring order for each week day:
            for (var i = 0; i < 5; i++)
            {
                var name = $"Test-({randomStr})-{i}";
                var weeklyRecurrence = WeeklyRecurrence.EveryWeek;
                var timeslotId = earlyMorningTimesltos[i].Id;

                var recurringOrder =
                    new RecurringOrder(
                        Guid.NewGuid(),
                        name,
                        contact.Id,
                        contact.DeliveryAddresses.FirstOrDefault().Id,
                        contact.BillingAddresses.FirstOrDefault().Id,
                        startDate,
                        Date.MaxDate.Value,
                        weeklyRecurrence,
                        timeslotId)
                    {
                        OrderItems = new List<RecurringOrderItem>
                            {
                            new RecurringOrderItem { ProductId = products[0].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 30 }
                            }
                    };

                await fedClient.CreateRecurringOrderAsync(recurringOrder);
            }

            // Create additional recurring orders:
            var tuesdayTimeslotId = earlyMorningTimesltos[1].Id;

            var recurringOrderX =
                new RecurringOrder(
                    Guid.NewGuid(),
                    $"Test-({randomStr})-5",
                    contact.Id,
                    contact.DeliveryAddresses.FirstOrDefault().Id,
                    contact.BillingAddresses.FirstOrDefault().Id,
                    startDate,
                    Date.MaxDate.Value,
                    WeeklyRecurrence.EverySecondWeek,
                    tuesdayTimeslotId)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[0].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 30 }
                        }
                };

            await fedClient.CreateRecurringOrderAsync(recurringOrderX);

            var thursdayTimeslotId = earlyMorningTimesltos[3].Id;

            var recurringOrderY =
                new RecurringOrder(
                    Guid.NewGuid(),
                    $"Test-({randomStr})-6",
                    contact.Id,
                    contact.DeliveryAddresses.FirstOrDefault().Id,
                    contact.BillingAddresses.FirstOrDefault().Id,
                    startDate,
                    Date.MaxDate.Value,
                    WeeklyRecurrence.EveryThirdWeek,
                    thursdayTimeslotId)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[0].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 30 }
                        }
                };

            await fedClient.CreateRecurringOrderAsync(recurringOrderY);

            // Get forecast:
            var forecast = await fedClient.GetForecastAsync(startDate, startDate.AddMonths(1).AddDays(-1));

            // Filter recurring orders down to the ones created by this test run:
            var keys = forecast.Keys.ToList();
            foreach (var key in keys)
            {
                var recurringOrders = forecast[key];
                var filteredRecurringOrders = recurringOrders.Where(o => o.Name.StartsWith($"Test-({randomStr})-")).ToList();
                forecast[key] = filteredRecurringOrders;
            }

            // Expected forecast:
            //- 2030/01/01 (TUE): 1, 5
            //- 2030/01/02 (WED): 2
            //- 2030/01/03 (THU): 3, 6
            //- 2030/01/04 (FRI): 4

            //- 2030/01/07 (MON): 0
            //- 2030/01/08 (TUE): 1
            //- 2030/01/09 (WED): 2
            //- 2030/01/10 (THU): 3
            //- 2030/01/11 (FRI): 4

            //- 2030/01/14 (MON): 0
            //- 2030/01/15 (TUE): 1, 5
            //- 2030/01/16 (WED): 2
            //- 2030/01/17 (THU): 3
            //- 2030/01/18 (FRI): 4

            //- 2030/01/21 (MON): 0
            //- 2030/01/22 (TUE): 1
            //- 2030/01/23 (WED): 2
            //- 2030/01/24 (THU): 3, 6
            //- 2030/01/25 (FRI): 4

            //- 2030/01/28 (MON): 0
            //- 2030/01/29 (TUE): 1, 5
            //- 2030/01/30 (WED): 2
            //- 2030/01/31 (THU): 3

            var expectedResult =
                    new Dictionary<string, int[]>
                    {
                        { "2030/01/01", new int[] { 1, 5 } },
                        { "2030/01/02", new int[] { 2 } },
                        { "2030/01/03", new int[] { 3, 6 } },
                        { "2030/01/04", new int[] { 4 } },
                        { "2030/01/07", new int[] { 0 } },
                        { "2030/01/08", new int[] { 1 } },
                        { "2030/01/09", new int[] { 2 } },
                        { "2030/01/10", new int[] { 3 } },
                        { "2030/01/11", new int[] { 4 } },
                        { "2030/01/14", new int[] { 0 } },
                        { "2030/01/15", new int[] { 1, 5 } },
                        { "2030/01/16", new int[] { 2 } },
                        { "2030/01/17", new int[] { 3 } },
                        { "2030/01/18", new int[] { 4 } },
                        { "2030/01/21", new int[] { 0 } },
                        { "2030/01/22", new int[] { 1 } },
                        { "2030/01/23", new int[] { 2 } },
                        { "2030/01/24", new int[] { 3, 6 } },
                        { "2030/01/25", new int[] { 4 } },
                        { "2030/01/28", new int[] { 0 } },
                        { "2030/01/29", new int[] { 1, 5 } },
                        { "2030/01/30", new int[] { 2 } },
                        { "2030/01/31", new int[] { 3 } }
                    };

            Assert.Equal(23, forecast.Count);

            foreach (var (key, ids) in expectedResult)
            {
                var date = Date.Parse(key);
                var recurringOrders = forecast[date];

                Assert.Equal(ids.Length, recurringOrders.Count);

                foreach (var id in ids)
                    Assert.Contains(recurringOrders, o => o.Name.Equals($"Test-({randomStr})-{id}"));
            }
        }

        [Fact]
        public async Task ForecastDateInPastTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            DateTime deliveryDate = DateTime.Now.GetPreviousWorkingDay();

            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var products = await fedClient.GetProductsAsync(null);

            var hubId = hubs[0].Id;
            var timeslotId = timeslots.FirstOrDefault(t => t.DayOfWeek == deliveryDate.DayOfWeek).Id;

            var customer = await fedClient.CreateCustomerAsync(TestDataBuilder.BuildCustomerWithContact(hubId));

            var recurringOrder = TestDataBuilder.BuildRecurringOrder(customer.PrimaryContact, timeslotId, 1);
            recurringOrder.Name = "Forecast Date In Past Test";
            recurringOrder.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products, 1);

            var createdOrder = await fedClient.CreateRecurringOrderAsync(recurringOrder);
            createdOrder.StartDate = deliveryDate;
            createdOrder.EndDate = deliveryDate.AddWeeks(2);
            var updatedOrder = await fedClient.UpdateRecurringOrderAsync(createdOrder.Id, createdOrder);

            await fedClient.DeleteDeliveriesAsync(deliveryDate);

            var forecast = await fedClient.GetForecastAsync(deliveryDate, deliveryDate);

            Assert.True(forecast.ContainsKey(deliveryDate));

        }
    }
}
