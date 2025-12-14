using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Web.Service.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Fed.Tests.Common;
using Fed.Core.Models;
using System.Collections.Generic;

namespace Fed.Tests.Integration
{
    [Collection("DeliveryTests")]
    public class DiscountsTests
    {
        [Fact]
        public async Task DiscountMaxValueTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());
            var hubs = await fedClient.GetHubsAsync();
            var hubId = hubs[0].Id;
            var timeslots = await fedClient.GetTimeslotsAsync(hubId);
            var products = await fedClient.GetProductsAsync(null);
            var deliveryDayOfWeek = DayOfWeek.Wednesday;
            var deliveryDate = DateTime.Today.AddDays(TestDataBuilder.GetRandomInt(1, 30)).AddYears(TestDataBuilder.GetRandomInt(5, 10)).EquivalentWeekDay(deliveryDayOfWeek);

            var discount = new Discount(
                new Guid(),
                $"Test Discount {TestDataBuilder.GetRandomName()}",
                "Test discount for integration test",
                DiscountRewardType.Percentage,
                DiscountQualificationType.OrderValue,
                DiscountEligibleProductsType.AllProducts,
                25,
                null,
                20,
                200,
                false,
                false,
                DiscountEvent.Manual,
                DateTime.Today,
                deliveryDate.AddYears(1),
                DiscountEvent.Manual,
                null,
                null,
                null,
                null,
                null,
                null,
                null
                );


            var createdDiscount = await fedClient.CreateDiscountAsync(discount);

            var customer = TestDataBuilder.BuildCustomer();
            customer.Contacts.Add(TestDataBuilder.BuildContact(customer.Id, hubId));

            var createdCustomer = await fedClient.CreateCustomerAsync(customer);
            var createdContact = createdCustomer.Contacts.Single();

            var applied = await fedClient.ApplyDiscountAsync(createdDiscount.Id, createdCustomer.Id);

            Assert.True(applied);

            var timeslotId = timeslots.FirstOrDefault(t => t.DayOfWeek == deliveryDayOfWeek).Id;

            var orderProduct = products.Where(p => p.Price < discount.MinOrderValue).First();

            var recurringOrderUnderMax = TestDataBuilder.BuildRecurringOrder(createdContact, timeslotId, 1);
            recurringOrderUnderMax.OrderItems.Add(TestDataBuilder.BuildRecurringOrderItem(orderProduct, 1));

            var recurringOrderOverMax1 = TestDataBuilder.BuildRecurringOrder(createdContact, timeslotId, 1);
            recurringOrderOverMax1.OrderItems.Add(TestDataBuilder.BuildRecurringOrderItem(orderProduct, 1));

            var recurringOrderOverMax2 = TestDataBuilder.BuildRecurringOrder(createdContact, timeslotId, 1);
            recurringOrderOverMax2.OrderItems.Add(TestDataBuilder.BuildRecurringOrderItem(orderProduct, 1));

            if (recurringOrderUnderMax.TotalItemPrice < discount.MinOrderValue)
                do
                {
                    recurringOrderUnderMax.OrderItems.First().Quantity++;

                } while (recurringOrderUnderMax.TotalItemPrice <= discount.MinOrderValue + 50);

            do
            {
                recurringOrderOverMax1.OrderItems.First().Quantity++;

            } while (recurringOrderOverMax1.TotalItemPrice <= discount.MaxOrderValue + 50);

            do
            {
                recurringOrderOverMax2.OrderItems.First().Quantity++;

            } while (recurringOrderOverMax2.TotalItemPrice <= discount.MaxOrderValue + 100);

            //place orders
            var createdRecurringOrderUnderMax = await fedClient.CreateRecurringOrderAsync(recurringOrderUnderMax);
            var createdRecurringOrderOverMax1 = await fedClient.CreateRecurringOrderAsync(recurringOrderOverMax1);
            var createdRecurringOrderOverMax2 = await fedClient.CreateRecurringOrderAsync(recurringOrderOverMax2);

            var generatedOrders = await fedClient.PlaceOrdersAsync(deliveryDate);
            var generatedDeliveries = await fedClient.CreateDeliveriesAsync(deliveryDate);

            var contactDeliveries = generatedDeliveries.Where(d => d.ContactId == createdContact.Id);

            var createdOrderUnderMax = await fedClient.GetOrderAsync(generatedOrders.Where(o => o.RecurringOrderId == createdRecurringOrderUnderMax.Id).Single().GeneratedOrderId);
            var createdOrderOverMax1 = await fedClient.GetOrderAsync(generatedOrders.Where(o => o.RecurringOrderId == createdRecurringOrderOverMax1.Id).Single().GeneratedOrderId);
            var createdOrderOverMax2 = await fedClient.GetOrderAsync(generatedOrders.Where(o => o.RecurringOrderId == createdRecurringOrderOverMax2.Id).Single().GeneratedOrderId);

            var createdOrderUnderMaxDiscount = createdOrderUnderMax.OrderDiscounts.Where(d => d.DiscountId == createdDiscount.Id).Single();
            var createdOrderOverMax1Discount = createdOrderOverMax1.OrderDiscounts.Where(d => d.DiscountId == createdDiscount.Id).Single();
            var createdOrderOverMax2Discount = createdOrderOverMax1.OrderDiscounts.Where(d => d.DiscountId == createdDiscount.Id).Single();

            Assert.True(createdOrderUnderMaxDiscount.OrderTotalDeduction < createdOrderOverMax1Discount.OrderTotalDeduction);
            Assert.True(createdOrderUnderMaxDiscount.OrderTotalDeduction < createdOrderOverMax2Discount.OrderTotalDeduction);
            Assert.Equal(createdOrderOverMax1Discount.OrderTotalDeduction, createdOrderOverMax2Discount.OrderTotalDeduction);

            //tidy up
            await fedClient.DeleteDeliveriesAsync(deliveryDate);

        }

        [Fact]
        public async Task CRUDDiscountTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var discounts = await fedClient.GetDiscountsAsync();

            Assert.NotEmpty(discounts);

            string name = TestDataBuilder.GetRandomName();
            string description = TestDataBuilder.GetRandomString();

            Discount newDiscount = new Discount(
                Guid.Empty,
                name,
                description,
                DiscountRewardType.Percentage,
                DiscountQualificationType.OrderValue,
                DiscountEligibleProductsType.AllProducts,
                10,
                null,
                10,
                200,
                false,
                false,
                DiscountEvent.Manual,
                DateTime.Now,
                DateTime.Now.AddDays(1),
                DiscountEvent.Manual,
                null,
                null,
                null,
                null,
                null,
                null,
                null
                );

            var createdDiscount = await fedClient.CreateDiscountAsync(newDiscount);

            Assert.Equal(name, createdDiscount.Name);

            string newName = TestDataBuilder.GetRandomName();

            var patchName = PatchOperation.CreateReplace("/name", newName);

            await fedClient.PatchDiscountAsync(createdDiscount.Id, patchName);

            var updatedDiscount = await fedClient.GetDiscountAsync(createdDiscount.Id);

            Assert.Equal(newName, updatedDiscount.Name);

        }

        [Fact]
        public async Task DiscountedProductsAddedToOrderTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            string description = TestDataBuilder.GetRandomString();

            var products = await fedClient.GetProductsAsync(null);

            products.Shuffle<Product>();

            var hubs = await fedClient.GetHubsAsync();
            var timeslots = await fedClient.GetTimeslotsAsync(hubs[0].Id);

            var deliveryDayOfWeek = DayOfWeek.Monday;
            DateTime deliveryDate = DateTime.Now.AddYears(10).EquivalentWeekDay(deliveryDayOfWeek);           

            var hubId = hubs[0].Id;
            var timeslot = timeslots.FirstOrDefault(t => t.DayOfWeek == deliveryDayOfWeek);

            var customer = TestDataBuilder.BuildCustomer();
            customer.IsDeliveryChargeExempt = false;
            var contact = TestDataBuilder.BuildContact(customer.Id, hubId);
            customer.Contacts.Add(contact);

            var createdCustomer = await fedClient.CreateCustomerAsync(customer);
            var createdContact = createdCustomer.Contacts.Single();

            //apply discount
            

            //set up one off order
            var order = TestDataBuilder.BuildRecurringOrder(createdContact, timeslot.Id, 0);
            order.StartDate = deliveryDate;
            order.OrderItems = TestDataBuilder.BuildRecurringOrderItems(products);

            var p = products.Where(p => p.Id == order.OrderItems.First().ProductId).Single();

            var discountedProducts = new List<LineItem>()
            {
                new LineItem
                {
                    ProductCode = p.ProductCode,
                    Quantity = 1,
                    Price = 0
                }
            };

            Discount productDiscount = new Discount(
                Guid.Empty,
                "Free product",
                description,
                DiscountRewardType.Product,
                DiscountQualificationType.OrderValue,
                DiscountEligibleProductsType.AllProducts,
                10,
                null,
                10,
                200,
                false,
                false,
                DiscountEvent.Manual,
                deliveryDate,
                deliveryDate.AddYears(1),
                DiscountEvent.Manual,
                null,
                null,
                null,
                null,
                null,
                discountedProducts,
                null
                );

            Discount percentageDiscount = new Discount(
                Guid.Empty,
                "PercentageOff",
                description,
                DiscountRewardType.Percentage,
                DiscountQualificationType.OrderValue,
                DiscountEligibleProductsType.AllProducts,
                20,
                null,
                10,
                200,
                false,
                false,
                DiscountEvent.Manual,
                DateTime.Now,
                deliveryDate.AddYears(1),
                DiscountEvent.Manual,
                null,
                null,
                null,
                null,
                null,
                null,
                null
                );

            var createdProductDiscount = await fedClient.CreateDiscountAsync(productDiscount);
            var createdPercentageDiscount = await fedClient.CreateDiscountAsync(percentageDiscount);
            var appliedProduct = await fedClient.ApplyDiscountAsync(createdProductDiscount.Id, createdCustomer.Id);
            var appliedPercentage = await fedClient.ApplyDiscountAsync(createdPercentageDiscount.Id, createdCustomer.Id);

            var createdContactFreeOrder = await fedClient.CreateRecurringOrderAsync(order);

            await fedClient.DeleteDeliveriesAsync(deliveryDate);
            var generatedOrders = await fedClient.PlaceOrdersAsync(deliveryDate);
            var generatedDeliveries = await fedClient.CreateDeliveriesAsync(deliveryDate);

            //var discountedOrder = generatedDeliveries.Where(d => d.ContactId == createdContact.Id).Single().Orders.Single();

            //string newName = TestDataBuilder.GetRandomName();

            //var patchName = PatchOperation.CreateReplace("/name", newName);

            //await fedClient.PatchDiscountAsync(createdDiscount.Id, patchName);

            //var updatedDiscount = await fedClient.GetDiscountAsync(createdDiscount.Id);

            //Assert.Equal(newName, updatedDiscount.Name);

        }
    }
}
