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
    public class DeliveryTests
    {
        [Fact]
        public async Task SplitAndGroupedDeliveryTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            // Get a hub ID first:
            var hubs = await fedClient.GetHubsAsync();
            var hubId = hubs[0].Id;

            // Create some customers:
            var companyName1 = $"Test-{Guid.NewGuid().ToString().Substring(20)}";
            var splitDeliveryCustomer =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName1,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    true,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null)
                {
                    Contacts = new[]
                    {
                        new Contact(
                            Guid.Empty,
                            "",
                            Guid.Empty,
                            "",
                            "Christina",
                            "Lanson",
                            TestDataBuilder.GetRandomEmail(),
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Christina Lanson",
                                    companyName1,
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "5th Floor",
                                    "London",
                                    "EC1V 2NX",
                                    "Please deliver to 5th floor.",
                                    "+44 (0) 74305 93022",
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Christina Lanson",
                                    companyName1,
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "5th Floor",
                                    "London",
                                    "EC1V 2NX",
                                    "office@examle.org",
                                    "12345985",
                                    "PO1234")
                            },
                            new[]
                            {
                                new CardToken(
                                    Guid.Empty, Guid.Empty,
                                    10,
                                    2020,
                                    "2345 **** **** ****",
                                    "Christina Lanson",
                                    "Ramon Lee & Partners Kemp House, Office, City Rd",
                                    "EC1V 2NX",
                                    true)
                            })
                    }
                };

            var companyName2 = $"Test-{Guid.NewGuid().ToString().Substring(20)}";
            var groupedDeliveryCustomer =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName2,
                    "",
                    null,
                    true,
                    10,
                    100,
                    false,
                    false,
                    false,
                    AccountType.Standard,
                    null,
                    null,
                    false,
                    null)
                {
                    Contacts = new[]
                     {
                        new Contact(
                            Guid.Empty,
                            "",
                            Guid.Empty,
                            "",
                            "Oliver",
                            "R. Phillips",
                            TestDataBuilder.GetRandomEmail(),
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Oliver R. Phillips",
                                    companyName2,
                                    "133 Whitechapel High Street",
                                    "",
                                    "London",
                                    "E1 7QA",
                                    "",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Oliver R. Phillips",
                                    companyName2,
                                    "133 Whitechapel High Street",
                                    "",
                                    "London",
                                    "E1 7QA",
                                    "office@examle.org",
                                    "12345985",
                                    "PO1234")
                            },
                            new[]
                            {
                                new CardToken(
                                    Guid.Empty, Guid.Empty,
                                    10,
                                    2020,
                                    "2345 **** **** ****",
                                    "Oliver R. Phillips",
                                    "133 Whitechapel High Street",
                                    "E1 7QA",
                                    true)
                            })
                     }
                };

            splitDeliveryCustomer = await fedClient.CreateCustomerAsync(splitDeliveryCustomer);
            groupedDeliveryCustomer = await fedClient.CreateCustomerAsync(groupedDeliveryCustomer);

            // Then create some recurring orders:

            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var products = await fedClient.GetProductsAsync(null);

            var timeslotId = timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Thursday).Id;

            var recurringOrderSplitA =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Thursday Kitchen",
                    splitDeliveryCustomer.Contacts[0].Id,
                    splitDeliveryCustomer.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    splitDeliveryCustomer.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2120, 01, 01),
                    Date.Create(2121, 01, 06),
                    WeeklyRecurrence.EveryWeek,
                    timeslotId)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[0].Id, Quantity = 45 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[5].Id, Quantity = 5 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 100 }
                        }
                };

            var recurringOrderSplitB =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Thursday Floor 1",
                    splitDeliveryCustomer.Contacts[0].Id,
                    splitDeliveryCustomer.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    splitDeliveryCustomer.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2120, 01, 01),
                    Date.Create(2121, 01, 06),
                    WeeklyRecurrence.EveryWeek,
                    timeslotId)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[0].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 40 }
                        }
                };

            var recurringOrderSplitC =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Thursday Floor 2",
                    splitDeliveryCustomer.Contacts[0].Id,
                    splitDeliveryCustomer.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    splitDeliveryCustomer.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2120, 01, 01),
                    Date.Create(2121, 01, 06),
                    WeeklyRecurrence.EveryWeek,
                    timeslotId)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[0].Id, Quantity = 45 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[5].Id, Quantity = 5 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 100 }
                        }
                };

            var recurringOrderSplitD =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Thursday Floor 3",
                    splitDeliveryCustomer.Contacts[0].Id,
                    splitDeliveryCustomer.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    splitDeliveryCustomer.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2120, 01, 01),
                    Date.Create(2121, 01, 06),
                    WeeklyRecurrence.EveryWeek,
                    timeslotId)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[0].Id, Quantity = 45 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[5].Id, Quantity = 5 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 100 }
                        }
                };

            var recurringOrderGroupedA =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Thursday Pre-Lunch Order",
                    groupedDeliveryCustomer.Contacts[0].Id,
                    groupedDeliveryCustomer.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    groupedDeliveryCustomer.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2120, 01, 01),
                    Date.Create(2121, 01, 06),
                    WeeklyRecurrence.EveryWeek,
                    timeslotId,
                    null, null, false, null)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 8 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 9 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 12 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 7 }
                        }
                };

            var recurringOrderGroupedB =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Thursday Topup",
                    groupedDeliveryCustomer.Contacts[0].Id,
                    groupedDeliveryCustomer.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    groupedDeliveryCustomer.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2120, 01, 01),
                    Date.Create(2121, 01, 06),
                    WeeklyRecurrence.EveryWeek,
                    timeslotId)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 4 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 4 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 4 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 3 }
                        }
                };

            var recurringOrderGroupedC =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Thursday Pre-Lunch Order",
                    groupedDeliveryCustomer.Contacts[0].Id,
                    groupedDeliveryCustomer.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    groupedDeliveryCustomer.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2120, 01, 01),
                    Date.Create(2121, 01, 06),
                    WeeklyRecurrence.EveryWeek,
                    timeslotId)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 8 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 9 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 12 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 7 }
                        }
                };

            var recurringOrderGroupedD =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Thursday Pre-Lunch Order",
                    groupedDeliveryCustomer.Contacts[0].Id,
                    groupedDeliveryCustomer.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    groupedDeliveryCustomer.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2120, 01, 01),
                    Date.Create(2121, 01, 06),
                    WeeklyRecurrence.EveryWeek,
                    timeslotId)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 8 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 9 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 12 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 7 }
                        }
                };

            recurringOrderSplitA = await fedClient.CreateRecurringOrderAsync(recurringOrderSplitA);
            recurringOrderSplitB = await fedClient.CreateRecurringOrderAsync(recurringOrderSplitB);
            recurringOrderSplitC = await fedClient.CreateRecurringOrderAsync(recurringOrderSplitC);
            recurringOrderSplitD = await fedClient.CreateRecurringOrderAsync(recurringOrderSplitD);
            recurringOrderGroupedA = await fedClient.CreateRecurringOrderAsync(recurringOrderGroupedA);
            recurringOrderGroupedB = await fedClient.CreateRecurringOrderAsync(recurringOrderGroupedB);
            recurringOrderGroupedC = await fedClient.CreateRecurringOrderAsync(recurringOrderGroupedC);
            recurringOrderGroupedD = await fedClient.CreateRecurringOrderAsync(recurringOrderGroupedD);



            //Place Orders and Deliveries for first Thursday of 2020
            DateTime deliveryDate = Date.Create(2120, 01, 01);
            while (deliveryDate.DayOfWeek != DayOfWeek.Thursday)
            {
                deliveryDate = deliveryDate.AddDays(1);
            }

            //clear any existing deliveries
            await fedClient.DeleteDeliveriesAsync(deliveryDate);
            // Create Deliveries for this date
            var generatedOrders = await fedClient.PlaceOrdersAsync(deliveryDate);
            var generatedDeliveries = await fedClient.CreateDeliveriesAsync(deliveryDate);

            var deliveries = await fedClient.GetDeliveriesAsync(deliveryDate);

            var splitDeliveries = deliveries.Where(d => d.ContactId == splitDeliveryCustomer.Contacts.First().Id);
            var groupedDeliveries = deliveries.Where(d => d.ContactId == groupedDeliveryCustomer.Contacts.First().Id);

            //we should have 4 separate split deliveries
            Assert.Equal(4, splitDeliveries.Count());

            //we should only have one delivery charge
            Assert.Single(splitDeliveries, d => d.DeliveryCharge > 0);

            //each split delivery should have a single order
            foreach (var delivery in splitDeliveries)
                Assert.Single(delivery.Orders);


            //we should have a single group delivery
            Assert.Single(groupedDeliveries);

            //this delivery should have 4 orders
            Assert.Equal(4, groupedDeliveries.Single().Orders.Count());

            //tidy up
            await fedClient.DeleteDeliveriesAsync(deliveryDate);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        public async Task MultipleDeliveryAddressesTest(int numberOfDilveryAddresses)
        {

            var fedClient = FedWebClient.Create(new NullLogger());

            var deliveryDayOfWeek = DayOfWeek.Monday;
            DateTime deliveryDate = DateTime.Now.AddYears(1).EquivalentWeekDay(deliveryDayOfWeek);

            // Get a hub ID first:
            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var products = await fedClient.GetProductsAsync(null);

            var hubId = hubs[0].Id;
            var timeslotId = timeslots.FirstOrDefault(t => t.DayOfWeek == deliveryDayOfWeek).Id;

            var customer = TestDataBuilder.BuildCustomer();
            var contact = TestDataBuilder.BuildContact(customer.Id, hubId);


            //give the contact multiple delivery addresses
            do
            {
                contact.DeliveryAddresses.Add(TestDataBuilder.BuildDeliveryAddress(contact.Id, false, hubId));
            }
            while (contact.DeliveryAddresses.Count < numberOfDilveryAddresses);

            customer.Contacts.Add(contact);

            //create customer in repo
            var createdCustomer = await fedClient.CreateCustomerAsync(customer);
            var createdContact = createdCustomer.Contacts.Single();
            var createdBillingAddress = createdContact.BillingAddresses.Where(b => b.IsPrimary).FirstOrDefault();


            //create a recurring order for each delivery address
            foreach (var deliveryAddress in createdContact.DeliveryAddresses)
            {
                //create three orders per delivery address
                for (int i = 0; i < 3; i++)
                {
                    var order = TestDataBuilder.BuildRecurringOrder(createdContact.Id, deliveryAddress.Id, createdBillingAddress.Id, Guid.Empty, timeslotId, 1);
                    order.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products);
                    var createdOrder = await fedClient.CreateRecurringOrderAsync(order);
                }
            }



            //clear any existing deliveries
            await fedClient.DeleteDeliveriesAsync(deliveryDate);
            // Create Deliveries for this date
            var generatedOrders = await fedClient.PlaceOrdersAsync(deliveryDate);
            var generatedDeliveries = await fedClient.CreateDeliveriesAsync(deliveryDate);

            var contactDeliveries = generatedDeliveries.Where(d => d.ContactId == createdContact.Id);

            Assert.Equal(numberOfDilveryAddresses, contactDeliveries.Count());

            foreach (var deliveryAddress in createdContact.DeliveryAddresses)
            {
                Assert.True(contactDeliveries.Where(d => d.DeliveryAddressId == deliveryAddress.Id).Any());
            }

            //tidy up
            await fedClient.DeleteDeliveriesAsync(deliveryDate);
        }

        [Fact]
        public async Task FreeOrdersDoNotHaveDeliveryChargeTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var deliveryDayOfWeek = DayOfWeek.Monday;
            DateTime deliveryDate = DateTime.Now.AddYears(10).EquivalentWeekDay(deliveryDayOfWeek);


            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var products = await fedClient.GetProductsAsync(null);

            var hubId = hubs[0].Id;
            var timeslot = timeslots.FirstOrDefault(t => t.DayOfWeek == deliveryDayOfWeek);


            //set up customers
            var customer1 = TestDataBuilder.BuildCustomer();
            customer1.IsDeliveryChargeExempt = false;
            var contact1 = TestDataBuilder.BuildContact(customer1.Id, hubId);
            customer1.Contacts.Add(contact1);

            var createdCustomer1 = await fedClient.CreateCustomerAsync(customer1);
            var createdContact1 = createdCustomer1.Contacts.Single();

            var customer2 = TestDataBuilder.BuildCustomer();
            customer2.IsDeliveryChargeExempt = false;
            var contact2 = TestDataBuilder.BuildContact(customer2.Id, hubId);
            customer2.Contacts.Add(contact2);

            var createdCustomer2 = await fedClient.CreateCustomerAsync(customer2);
            var createdContact2 = createdCustomer2.Contacts.Single();

            //set up one off orders
            var contact1FreeOrder1 = TestDataBuilder.BuildRecurringOrder(createdContact1, timeslot.Id, 0, true);
            contact1FreeOrder1.StartDate = deliveryDate;
            contact1FreeOrder1.EndDate = deliveryDate;
            contact1FreeOrder1.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products);

            var contact1FreeOrder2 = TestDataBuilder.BuildRecurringOrder(createdContact1, timeslot.Id, 0, true);
            contact1FreeOrder2.StartDate = deliveryDate;
            contact1FreeOrder2.EndDate = deliveryDate;
            contact1FreeOrder2.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products);

            var contact1FreeOrder3 = TestDataBuilder.BuildRecurringOrder(createdContact1, timeslot.Id, 0, true);
            contact1FreeOrder3.StartDate = deliveryDate;
            contact1FreeOrder3.EndDate = deliveryDate;
            contact1FreeOrder3.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products);

            var contact2FreeOrder1 = TestDataBuilder.BuildRecurringOrder(createdContact2, timeslot.Id, 0, true);
            contact2FreeOrder1.StartDate = deliveryDate;
            contact2FreeOrder1.EndDate = deliveryDate;
            contact2FreeOrder1.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products);

            var contact2NonFreeOrder1 = TestDataBuilder.BuildRecurringOrder(createdContact2, timeslot.Id, 0, false);
            contact2NonFreeOrder1.StartDate = deliveryDate;
            contact2NonFreeOrder1.EndDate = deliveryDate;
            contact2NonFreeOrder1.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products);

            var contact2NonFreeOrder2 = TestDataBuilder.BuildRecurringOrder(createdContact2, timeslot.Id, 0, false);
            contact2NonFreeOrder2.StartDate = deliveryDate;
            contact2NonFreeOrder2.EndDate = deliveryDate;
            contact2NonFreeOrder2.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products);

            //create orders
            var createdContact1FreeOrder1 = await fedClient.CreateRecurringOrderAsync(contact1FreeOrder1);
            var createdContact1FreeOrder2 = await fedClient.CreateRecurringOrderAsync(contact1FreeOrder2);
            var createdContact1FreeOrder3 = await fedClient.CreateRecurringOrderAsync(contact1FreeOrder3);

            var createdContact2FreeOrder1 = await fedClient.CreateRecurringOrderAsync(contact2FreeOrder1);
            var createdContact2NonFreeOrder1 = await fedClient.CreateRecurringOrderAsync(contact2NonFreeOrder1);
            var createdContact2NonFreeOrder2 = await fedClient.CreateRecurringOrderAsync(contact2NonFreeOrder2);


            // Create Deliveries for this date
            await fedClient.DeleteDeliveriesAsync(deliveryDate);
            var generatedOrders = await fedClient.PlaceOrdersAsync(deliveryDate);
            var generatedDeliveries = await fedClient.CreateDeliveriesAsync(deliveryDate);

            //get delivery for both contacts
            var contact1Delivery = generatedDeliveries.Where(d => d.ContactId == createdContact1.Id).Single();
            var contact2Delivery = generatedDeliveries.Where(d => d.ContactId == createdContact2.Id).Single();

            //Delivery with only free orders shouldn't have delivery charge
            Assert.Equal(0.00M, contact1Delivery.DeliveryCharge);

            //Delivery with free and chargeable orders should have delivery charge
            Assert.Equal(timeslot.DeliveryCharge, contact2Delivery.DeliveryCharge);

            //tidy up
            await fedClient.DeleteDeliveriesAsync(deliveryDate);
        }
    }
}
