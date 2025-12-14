using Fed.Core.Entities;
using Fed.Core.Enums;
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
    public class RecurringOrdersTests
    {
        [Fact]
        public async Task CRUDSkipDates()
        {
            var fedClient = FedWebClient.Create(new NullLogger());
            var customers = await fedClient.GetCustomersAsync(true);
            var contact = customers.First(c => c.Contacts[0].BillingAddresses != null && c.Contacts[0].DeliveryAddresses != null).Contacts[0];
            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var products = await fedClient.GetProductsAsync(null);

            // ----------------------------
            // POST /recurringorders
            // ----------------------------

            var randomId = Guid.NewGuid().ToString("N").Substring(0, 6);
            var name = $"Test Order {randomId}";
            var startDate = new DateTime(2021, 01, 01);
            var weeklyRecurrence = WeeklyRecurrence.EveryWeek;
            var timeslotId = timeslots[0].Id;

            var recurringOrder =
                new RecurringOrder(
                    Guid.NewGuid(),
                    name,
                    contact.Id,
                    contact.DeliveryAddresses.FirstOrDefault().Id,
                    contact.BillingAddresses.FirstOrDefault().Id,
                    startDate,
                    Date.Create(2021, 01, 01),
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

            var createdRecurringOrder = await fedClient.CreateRecurringOrderAsync(recurringOrder);

            // ----------------------------
            // PUT /recurringorders/{id}/skipdates/{date}
            // ----------------------------

            var skipDay = Date.Today.AddWeeks(1);
            var skipDates = await fedClient.SetSkipDateAsync(createdRecurringOrder.Id, skipDay, "Integration Test", "Test Runner");

            Assert.Contains(skipDay, skipDates.Select(s => s.Date));

            // ----------------------------
            // DELETE /recurringorders/{id}/skipdates/{date}
            // ----------------------------

            skipDates = await fedClient.DeleteSkipDateAsync(createdRecurringOrder.Id, skipDay);

            Assert.DoesNotContain(skipDay, skipDates.Select(s => s.Date));
        }

        [Fact]
        public async Task CRUDRecurringOrder()
        {
            var fedClient = FedWebClient.Create(new NullLogger());
            var customers = await fedClient.GetCustomersAsync(true);
            var contact = customers.First(c => c.Contacts[0].BillingAddresses != null && c.Contacts[0].DeliveryAddresses != null).Contacts[0];
            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var products = await fedClient.GetProductsAsync(null);

            // ----------------------------
            // POST /recurringorders
            // ----------------------------

            var randomId = Guid.NewGuid().ToString("N").Substring(0, 6);
            var name = $"Test Order {randomId}";
            var startDate = Date.Create(new DateTime(2120, 01, 01));
            var endDate = Date.Create(new DateTime(2121, 01, 06));
            var weeklyRecurrence = WeeklyRecurrence.EveryWeek;
            var timeslotId = timeslots[0].Id;
            var isFree = true;

            var recurringOrder =
                new RecurringOrder(
                    Guid.NewGuid(),
                    name,
                    contact.Id,
                    contact.DeliveryAddresses.FirstOrDefault().Id,
                    contact.BillingAddresses.FirstOrDefault().Id,
                    startDate,
                    endDate,
                    weeklyRecurrence,
                    timeslotId,
                    null, null, false, null, null, null, null, null, null,
                    isFree)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[0].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 30 }
                        }
                };

            var createdRecurringOrder = await fedClient.CreateRecurringOrderAsync(recurringOrder);

            Assert.Equal(name, createdRecurringOrder.Name);
            Assert.Equal(contact.Id, createdRecurringOrder.ContactId);
            Assert.Equal(startDate, createdRecurringOrder.StartDate);
            Assert.Equal(endDate, createdRecurringOrder.EndDate.Value);
            Assert.Equal(weeklyRecurrence, createdRecurringOrder.WeeklyRecurrence);
            Assert.Equal(timeslotId, createdRecurringOrder.TimeslotId);
            Assert.Equal(3, createdRecurringOrder.OrderItems.Count());
            Assert.True(createdRecurringOrder.IsFree);
            Assert.Contains(createdRecurringOrder.OrderItems, i => i.ProductId == products[0].Id && i.Quantity == 10);
            Assert.Contains(createdRecurringOrder.OrderItems, i => i.ProductId == products[2].Id && i.Quantity == 20);
            Assert.Contains(createdRecurringOrder.OrderItems, i => i.ProductId == products[4].Id && i.Quantity == 30);

            // ----------------------------
            // PUT /recurringorders/{id}
            // ----------------------------

            var updatedName = $"{name}-{DateTime.Now.Ticks}";
            createdRecurringOrder.Name = updatedName;
            createdRecurringOrder.WeeklyRecurrence = WeeklyRecurrence.EverySecondWeek;
            createdRecurringOrder.OrderItems.ToList().ForEach(i => i.Quantity += 200);

            await fedClient.UpdateRecurringOrderAsync(createdRecurringOrder.Id, createdRecurringOrder);

            // ----------------------------
            // GET /recurringorders/{id}
            // ----------------------------

            var updatedRecurringOrder = await fedClient.GetRecurringOrderAsync(createdRecurringOrder.Id);

            Assert.Equal(updatedName, updatedRecurringOrder.Name);
            Assert.Equal(contact.Id, updatedRecurringOrder.ContactId);
            Assert.Equal(startDate, updatedRecurringOrder.StartDate);
            Assert.Equal(endDate, updatedRecurringOrder.EndDate.Value);
            Assert.Equal(WeeklyRecurrence.EverySecondWeek, updatedRecurringOrder.WeeklyRecurrence);
            Assert.Equal(timeslotId, updatedRecurringOrder.TimeslotId);
            Assert.Equal(3, updatedRecurringOrder.OrderItems.Count());
            Assert.Contains(updatedRecurringOrder.OrderItems, i => i.ProductId == products[0].Id && i.Quantity == 210);
            Assert.Contains(updatedRecurringOrder.OrderItems, i => i.ProductId == products[2].Id && i.Quantity == 220);
            Assert.Contains(updatedRecurringOrder.OrderItems, i => i.ProductId == products[4].Id && i.Quantity == 230);

            // ----------------------------
            // PATCH /recurringorders/{id}
            // ----------------------------

            var patchedName = Guid.NewGuid().ToString("N").Substring(0, 10);
            var patchOperation = PatchOperation.CreateReplace("/name", patchedName);
            await fedClient.PatchRecurringOrderAsync(updatedRecurringOrder.Id, patchOperation);

            // ----------------------------
            // GET /recurringorders?contactId={contactId}
            // ----------------------------

            var allRecurringOrdersByContact = await fedClient.GetRecurringOrdersAsync(contact.Id, Date.MinDate, Date.MaxDate);
            var patchedRecurringOrder = allRecurringOrdersByContact.Single(r => r.Id == updatedRecurringOrder.Id);

            Assert.Equal(patchedName, patchedRecurringOrder.Name);
            Assert.Equal(contact.Id, patchedRecurringOrder.ContactId);
            Assert.Equal(startDate, patchedRecurringOrder.StartDate);
            Assert.Equal(endDate, patchedRecurringOrder.EndDate.Value);
            Assert.Equal(WeeklyRecurrence.EverySecondWeek, patchedRecurringOrder.WeeklyRecurrence);
            Assert.Equal(timeslotId, patchedRecurringOrder.TimeslotId);
            Assert.Equal(3, patchedRecurringOrder.OrderItems.Count());
            Assert.Contains(patchedRecurringOrder.OrderItems, i => i.ProductId == products[0].Id && i.Quantity == 210);
            Assert.Contains(patchedRecurringOrder.OrderItems, i => i.ProductId == products[2].Id && i.Quantity == 220);
            Assert.Contains(patchedRecurringOrder.OrderItems, i => i.ProductId == products[4].Id && i.Quantity == 230);
        }

        [Fact]
        public async Task UpdateRecurringOrderForOneDateOnly()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs.FirstOrDefault().Id);

            var products = await fedClient.GetProductsAsync(null);
            var customer = TestDataBuilder.BuildCustomer();
            var contact = TestDataBuilder.BuildContact(customer.Id, hubs.FirstOrDefault().Id);
            customer.Contacts.Add(contact);

            var createdCustomer = await fedClient.CreateCustomerAsync(customer);
            var createdContact = createdCustomer.Contacts.FirstOrDefault();

            //test on a Tuesday to avoid public holiday
            var tuesdayTimeslot = timeslots.Where(t => t.DayOfWeek == DayOfWeek.Tuesday).FirstOrDefault();


            var order = TestDataBuilder.BuildRecurringOrder(createdContact.Id,
                                                            createdContact.DeliveryAddresses.FirstOrDefault().Id,
                                                            createdContact.BillingAddresses.FirstOrDefault().Id,
                                                            createdContact.CardTokens.FirstOrDefault().Id,
                                                            tuesdayTimeslot.Id,
                                                            1);

            foreach (var p in products.Take(2))
            {
                order.OrderItems.Add(TestDataBuilder.BuildRecurringOrderItem(p));
            }

            var createdOrder = await fedClient.CreateRecurringOrderAsync(order);

            DateTime twoWeeksToday = DateTime.Today.AddDays(14);
            Date twoWeeksTuesday = twoWeeksToday.AddDays(((int)DayOfWeek.Tuesday - (int)twoWeeksToday.DayOfWeek + 7) % 7).Date;
            createdOrder.Name = createdOrder.Name + " Single order";

            var oneOffOrder = await fedClient.PutRecurringOrderForSingleDate(createdOrder.Id, twoWeeksTuesday, createdOrder);

            //get a forecast of contact's orders
            var forecast = await fedClient.GetForecastAsync(DateTime.Today, twoWeeksTuesday.AddDays(8), createdContact.Id);

            //check that forecast contains expected dates
            Assert.True(forecast.ContainsKey(twoWeeksTuesday.AddDays(-7)));
            Assert.True(forecast.ContainsKey(twoWeeksTuesday));
            Assert.True(forecast.ContainsKey(twoWeeksTuesday.AddDays(7)));

            //check that forecast recurring order falls either side of one off order
            Assert.Contains(forecast[twoWeeksTuesday.AddDays(-7)], o => o.Id == createdOrder.Id);
            Assert.Contains(forecast[twoWeeksTuesday.AddDays(7)], o => o.Id == createdOrder.Id);

            //check that one off order falls on expected date
            Assert.Contains(forecast[twoWeeksTuesday], o => o.Id == oneOffOrder.Id);

            //check that recurring order does not fall on expected date
            Assert.DoesNotContain(forecast[twoWeeksTuesday], o => o.Id == createdOrder.Id);
        }

        [Fact]
        public async Task UpdateRecurringOrderFromFirstDate()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs.FirstOrDefault().Id);

            var products = await fedClient.GetProductsAsync(null);
            var customer = TestDataBuilder.BuildCustomer();
            var contact = TestDataBuilder.BuildContact(customer.Id, hubs.FirstOrDefault().Id);
            customer.Contacts.Add(contact);

            var createdCustomer = await fedClient.CreateCustomerAsync(customer);
            var createdContact = createdCustomer.Contacts.FirstOrDefault();

            //test on a Tuesday to avoid public holiday
            var tuesdayTimeslot = timeslots.Where(t => t.DayOfWeek == DayOfWeek.Tuesday).FirstOrDefault();


            var order = TestDataBuilder.BuildRecurringOrder(createdContact.Id,
                                                            createdContact.DeliveryAddresses.FirstOrDefault().Id,
                                                            createdContact.BillingAddresses.FirstOrDefault().Id,
                                                            createdContact.CardTokens.FirstOrDefault().Id,
                                                            tuesdayTimeslot.Id,
                                                            1);

            foreach (var p in products.Take(2))
            {
                order.OrderItems.Add(TestDataBuilder.BuildRecurringOrderItem(p));
            }

            var createdOrder = await fedClient.CreateRecurringOrderAsync(order);


            //get a forecast of contact's orders
            var forecast = await fedClient.GetForecastAsync(DateTime.Today, DateTime.Today.AddDays((7 * 3) + 1), createdContact.Id);

            var nextDeliveryDate = DateTime.Now.Hour >= 16 && forecast.First().Key.Value == DateTime.Today.AddDays(1) ? forecast.ElementAt(1).Key : forecast.First().Key;

            var updatedOrder = await fedClient.PostRecurringOrderFromDate(createdOrder.Id, nextDeliveryDate, createdOrder);

            Assert.Equal(createdOrder.Id, updatedOrder.Id);

            var newStartDate = createdOrder.StartDate.AddWeeks(2);

            var newOrder = await fedClient.PostRecurringOrderFromDate(createdOrder.Id, newStartDate, createdOrder);

            Assert.NotEqual(updatedOrder.Id, newOrder.Id);
            Assert.Equal(newStartDate, newOrder.StartDate);
        }
    }
}
