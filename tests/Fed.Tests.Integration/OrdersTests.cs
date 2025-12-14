using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using Fed.Web.Service.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Fed.Tests.Common;

namespace Fed.Tests.Integration
{
    [Collection("DeliveryTests")]
    public class OrdersTests
    {
        [Fact]
        public async Task PlaceOrdersTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            // Get a hub ID first:
            var hubs = await fedClient.GetHubsAsync();
            var hubId = hubs[0].Id;

            // Create some customers:
            var companyName1 = $"Test-{Guid.NewGuid().ToString().Substring(20)}";
            var customer1 =
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
                                    TestDataBuilder.GetRandomEmail(),
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
            var customer2 =
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
                                    TestDataBuilder.GetRandomEmail(),
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

            var companyName3 = $"-Test-{Guid.NewGuid().ToString().Substring(20)}";
            var customer3 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName3,
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
                            "Wallace",
                            "Wilcox",
                            TestDataBuilder.GetRandomEmail(),
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Wallace Wilcox",
                                    companyName3,
                                    "5 Devonshire Square",
                                    "",
                                    "London",
                                    "EC2M 4YD",
                                    "",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Wallace Wilcox",
                                    companyName3,
                                    "5 Devonshire Square",
                                    "",
                                    "London",
                                    "EC2M 4YD",
                                    TestDataBuilder.GetRandomEmail(),
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
                                    "Wallace Wilcox",
                                    "5 Devonshire Square",
                                    "EC2M 4YD",
                                    true)
                            })
                     }
                };

            var companyName4 = $"Test-{Guid.NewGuid().ToString().Substring(20)}";
            var customer4 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName4,
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
                            "Gary",
                            "Parrish",
                            TestDataBuilder.GetRandomEmail(),
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Gary Parrish",
                                    companyName4,
                                    "10 Bury Street",
                                    "",
                                    "London",
                                    "EC3A 5AT",
                                    "Please use back entrance on Heneage Lane.",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Gary Parrish",
                                    companyName4,
                                    "10 Bury Street",
                                    "",
                                    "London",
                                    "EC3A 5AT",
                                    TestDataBuilder.GetRandomEmail(),
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
                                    "Gary Parrish",
                                    "10 Bury Street",
                                    "EC3A 5AT",
                                    true)
                            })
                     }
                };

            var companyName5 = $"Test-{Guid.NewGuid().ToString().Substring(20)}";
            var customer5 =
                new Customer(
                    Guid.Empty,
                    "",
                    companyName5,
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
                            "Sharon",
                            "Swanson",
                            TestDataBuilder.GetRandomEmail(),
                            "+44 (0) 74305 93021",
                            false, false,
                            (int)PaymentMethod.Card,
                            DateTime.UtcNow,
                            new[]
                            {
                                new DeliveryAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Sharon Swanson",
                                    companyName5,
                                    "110 Bishopsgate",
                                    "",
                                    "London",
                                    "EC2N 4AY",
                                    "Drop at reception downstairs.",
                                    Faker.Phone.Number(),
                                    false,
                                    hubId)
                            },
                            new[]
                            {
                                new BillingAddress(
                                    Guid.Empty, Guid.Empty, true,
                                    "Sharon Swanson",
                                    companyName5,
                                    "110 Bishopsgate",
                                    "",
                                    "London",
                                    "EC2N 4AY",
                                    TestDataBuilder.GetRandomEmail(),
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
                                    "Sharon Swanson",
                                    "110 Bishopsgate",
                                    "EC2N 4AY",
                                    true)
                            })
                     }
                };

            customer1 = await fedClient.CreateCustomerAsync(customer1);
            customer2 = await fedClient.CreateCustomerAsync(customer2);
            customer3 = await fedClient.CreateCustomerAsync(customer3);
            customer4 = await fedClient.CreateCustomerAsync(customer4);
            customer5 = await fedClient.CreateCustomerAsync(customer5);

            // Then create some recurring orders:

            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);
            var products = await fedClient.GetProductsAsync(null);

            var recurringOrder1A =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Monday Order",
                    customer1.Contacts[0].Id,
                    customer1.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Monday).Id)
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

            var recurringOrder1B =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Wednesday Topup",
                    customer1.Contacts[0].Id,
                    customer1.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Wednesday).Id)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[0].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 40 }
                        }
                };

            var recurringOrder2A =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Monday Pre-Lunch Order",
                    customer2.Contacts[0].Id,
                    customer2.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Monday).Id)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 8 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 9 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 12 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 7 }
                        }
                };

            var recurringOrder2B =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Wednesday Afternoon Topup",
                    customer2.Contacts[0].Id,
                    customer2.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Wednesday).Id)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 4 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 4 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 4 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 3 }
                        }
                };

            var recurringOrder3A =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Beginning of Week Order",
                    customer3.Contacts[0].Id,
                    customer3.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Monday).Id)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 11 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 12 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 13 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 14 },
                            new RecurringOrderItem { ProductId = products[5].Id, Quantity = 15 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 16 },
                            new RecurringOrderItem { ProductId = products[7].Id, Quantity = 17 }
                        }
                };

            var recurringOrder3B =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Mid-Week Topup",
                    customer3.Contacts[0].Id,
                    customer3.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Wednesday).Id)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 8 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 9 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 5 },
                            new RecurringOrderItem { ProductId = products[5].Id, Quantity = 5 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 6 },
                            new RecurringOrderItem { ProductId = products[7].Id, Quantity = 7 }
                        }
                };

            var recurringOrder4A =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Early Office Breakfast Order",
                    customer4.Contacts[0].Id,
                    customer4.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Monday).Id)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 11 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 12 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 14 },
                            new RecurringOrderItem { ProductId = products[5].Id, Quantity = 15 },
                            new RecurringOrderItem { ProductId = products[7].Id, Quantity = 17 }
                        }
                };

            var recurringOrder4B =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Wednesday Office Lunch Order",
                    customer4.Contacts[0].Id,
                    customer4.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Wednesday).Id)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 9 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 5 },
                            new RecurringOrderItem { ProductId = products[5].Id, Quantity = 5 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 6 }
                        }
                };

            var recurringOrder5A =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Monday Morning Feast",
                    customer5.Contacts[0].Id,
                    customer5.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Monday).Id)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 100 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 30 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 40 },
                            new RecurringOrderItem { ProductId = products[5].Id, Quantity = 50 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 60 },
                            new RecurringOrderItem { ProductId = products[7].Id, Quantity = 70 }
                        }
                };

            var recurringOrder5B =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Wednesday Morning Feast",
                    customer5.Contacts[0].Id,
                    customer5.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.EveryWeek,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Wednesday).Id)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 50 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 10 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 20 },
                            new RecurringOrderItem { ProductId = products[5].Id, Quantity = 30 },
                            new RecurringOrderItem { ProductId = products[6].Id, Quantity = 40 },
                            new RecurringOrderItem { ProductId = products[7].Id, Quantity = 30 }
                        }
                };

            recurringOrder1A = await fedClient.CreateRecurringOrderAsync(recurringOrder1A);
            recurringOrder1B = await fedClient.CreateRecurringOrderAsync(recurringOrder1B);
            recurringOrder2A = await fedClient.CreateRecurringOrderAsync(recurringOrder2A);
            recurringOrder2B = await fedClient.CreateRecurringOrderAsync(recurringOrder2B);
            recurringOrder3A = await fedClient.CreateRecurringOrderAsync(recurringOrder3A);
            recurringOrder3B = await fedClient.CreateRecurringOrderAsync(recurringOrder3B);
            recurringOrder4A = await fedClient.CreateRecurringOrderAsync(recurringOrder4A);
            recurringOrder4B = await fedClient.CreateRecurringOrderAsync(recurringOrder4B);
            recurringOrder5A = await fedClient.CreateRecurringOrderAsync(recurringOrder5A);
            recurringOrder5B = await fedClient.CreateRecurringOrderAsync(recurringOrder5B);

            // Create One-Off Orders

            var wednesdayOneOffOrderDate = Date.Create(2025, 01, 01);

            var oneOffOrder1A =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Wednesday One Off Order",
                    customer1.Contacts[0].Id,
                    customer1.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    wednesdayOneOffOrderDate,
                    wednesdayOneOffOrderDate,
                    WeeklyRecurrence.OneOff,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Wednesday).Id,
                    null, null, false, null, null, null, null, null, null, true)
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

            var oneOffOrder2B =
                new RecurringOrder(
                    Guid.NewGuid(),
                    "Wednesday Afternoon One Off",
                    customer2.Contacts[0].Id,
                    customer2.Contacts[0].DeliveryAddresses.FirstOrDefault().Id,
                    customer1.Contacts[0].BillingAddresses.FirstOrDefault().Id,
                    Date.Create(2025, 01, 01),
                    Date.Create(2026, 01, 01),
                    WeeklyRecurrence.OneOff,
                    timeslots.FirstOrDefault(t => t.DayOfWeek == DayOfWeek.Wednesday).Id,
                    null, null, false, null, null, null, null, null, null, false)
                {
                    OrderItems = new List<RecurringOrderItem>
                        {
                            new RecurringOrderItem { ProductId = products[1].Id, Quantity = 4 },
                            new RecurringOrderItem { ProductId = products[2].Id, Quantity = 4 },
                            new RecurringOrderItem { ProductId = products[3].Id, Quantity = 4 },
                            new RecurringOrderItem { ProductId = products[4].Id, Quantity = 3 }
                        }
                };

            oneOffOrder1A = await fedClient.CreateRecurringOrderAsync(oneOffOrder1A);
            oneOffOrder2B = await fedClient.CreateRecurringOrderAsync(oneOffOrder2B);

            // Place Orders and Deliveries for first week of 2020

            for (var day = 1; day <= 14; day++)
            {
                var date = Date.Create(2025, 01, day);
                await fedClient.PlaceOrdersAsync(date);
                await fedClient.CreateDeliveriesAsync(date);
            }

            // Create Deliveries for 01/01/2025
            var deliveryDate = Date.Create(2025, 01, 01);
            var deliveries = await fedClient.GetDeliveriesAsync(Date.Create(2025, 01, 01));

            //tidy up
            for (var day = 1; day <= 14; day++)
            {
                var date = Date.Create(2025, 01, day);
                await fedClient.DeleteDeliveriesAsync(date);
            }
        }

        [Fact]
        public async Task FreeRecurringOrdersGenerateFreeOrdersTest()
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
            var customer = TestDataBuilder.BuildCustomer();
            customer.IsDeliveryChargeExempt = false;
            var contact = TestDataBuilder.BuildContact(customer.Id, hubId);
            customer.Contacts.Add(contact);

            var createdCustomer = await fedClient.CreateCustomerAsync(customer);
            var createdContact = createdCustomer.Contacts.Single();

            //set up one off order
            var contactFreeOrder = TestDataBuilder.BuildRecurringOrder(createdContact, timeslot.Id, 0, true);
            contactFreeOrder.StartDate = deliveryDate;
            contactFreeOrder.EndDate = deliveryDate;
            contactFreeOrder.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products);

            var createdContactFreeOrder = await fedClient.CreateRecurringOrderAsync(contactFreeOrder);

            await fedClient.DeleteDeliveriesAsync(deliveryDate);
            var generatedOrders = await fedClient.PlaceOrdersAsync(deliveryDate);
            var generatedDeliveries = await fedClient.CreateDeliveriesAsync(deliveryDate);

            //get order for contact
            var order = generatedDeliveries.Where(d => d.ContactId == createdContact.Id).Single().Orders.Single();

            Assert.Equal(0, order.OrderItemsTotal);
            Assert.All(order.OrderItems, i => Assert.Equal(0, i.ActualPrice));

            //tidy up
            await fedClient.DeleteDeliveriesAsync(deliveryDate);
        }
    }
}
