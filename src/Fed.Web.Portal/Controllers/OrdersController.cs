using Fed.Core.Entities;
using Fed.Core.Extensions;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;

        public OrdersController(IConfiguration config, FedWebClient fedWebClient)
        {
            _config = config;
            _fedWebClient = fedWebClient;
        }

        [HttpGet("/orders/{customerId}/create")]
        public async Task<IActionResult> IndexAsync(string customerId)
        {
            var hubs = await _fedWebClient.GetHubsAsync();
            var customer = await _fedWebClient.GetCustomerFullInfoAsync(customerId);
            var deliveryAddresses = new List<DeliveryAddress>();
            foreach (var contact in customer.Contacts)
            {
                foreach (var address in contact.DeliveryAddresses.OrEmptyIfNull())
                {
                    deliveryAddresses.Add(address);
                }
            }
            var model = new Models.Orders.Order
            {
                OrderItems = new List<Models.Orders.OrderItem>(),
                Products = await _fedWebClient.GetProductsAsync(),
                TimeSlots = await _fedWebClient.GetTimeslotsAsync(hubs.FirstOrDefault().Id),
                CustomerId = customerId,
                ContactId = customer.PrimaryContact.Id.ToString(),
                DeliveryAddress = deliveryAddresses,
                BillingAddressId = customer.Contacts.FirstOrDefault().BillingAddresses?.FirstOrDefault()?.Id.ToString()
            };

            return View("CreateOrder", model);
        }

        [HttpPost("/orders/{customerId}/create")]
        public async Task<IActionResult> CreateOrderAsync(Models.Orders.Order order)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateOrder", order);
            }
            var orderItems = new List<RecurringOrderItem>();
            foreach (var orderItem in order.OrderItems)
            {
                orderItems.Add(new RecurringOrderItem()
                {
                    ProductId = orderItem.ProductId,
                    ProductCode = orderItem.ProductCode,
                    Quantity = int.Parse(orderItem.Quantity),
                    AddedDate = DateTime.Now
                });
            }

            var endDate = (Core.Enums.WeeklyRecurrence)(int.Parse(order.Reccurence)) == Core.Enums.WeeklyRecurrence.EveryWeek ? DateTime.Now.AddYears(100) : DateTime.Parse(order.StartDate);
            var recurringOrder = new RecurringOrder(new Guid(), order.OrderName,
                Guid.Parse(order.ContactId),
                Guid.Parse(order.DeliveryAddressId),
                Guid.Parse(order.BillingAddressId),
                DateTime.Parse(order.StartDate),
               endDate,
                (Core.Enums.WeeklyRecurrence)(int.Parse(order.Reccurence)),
                 Guid.Parse(order.TimeSlotId),
                orderItems: orderItems, isFree: order.FreeOrder);


            var newOrder = await _fedWebClient.CreateRecurringOrderAsync(recurringOrder);

            if (newOrder.Id != Guid.Empty)
            {
                ViewBag.Success = "1";
            }
            return RedirectToAction("DisplayCustomer", "Customers", new { customerId = order.CustomerId });
        }

        [HttpGet("/orders/{customerId}/{orderId}/edit")]
        public async Task<IActionResult> EditOrder(string customerId, string orderId)
        {
            var recurringOrder = await _fedWebClient.GetRecurringOrderAsync(Guid.Parse(orderId));
            if (recurringOrder != null)
            {
                var hubs = await _fedWebClient.GetHubsAsync();
                var customer = await _fedWebClient.GetCustomerFullInfoAsync(customerId);
                var deliveryAddresses = new List<DeliveryAddress>();
                foreach (var contact in customer.Contacts)
                {
                    foreach (var address in contact.DeliveryAddresses)
                    {
                        deliveryAddresses.Add(address);
                    }
                }
                var orderItems = new List<Models.Orders.OrderItem>();
                foreach (var recurringOrderItem in recurringOrder.OrderItems)
                {
                    orderItems.Add(new Models.Orders.OrderItem
                    {
                        ProductId = recurringOrderItem.ProductId,
                        ProductCode = recurringOrderItem.ProductCode,
                        Quantity = recurringOrderItem.Quantity.ToString(),
                        Price = recurringOrderItem.Price.ToString()
                    });
                }
                var order = new Models.Orders.Order
                {
                    Products = await _fedWebClient.GetProductsAsync(),
                    TimeSlots = await _fedWebClient.GetTimeslotsAsync(hubs.FirstOrDefault().Id),
                    CustomerId = customerId,
                    OrderId = orderId,
                    ContactId = customer.PrimaryContact.Id.ToString(),
                    DeliveryAddress = deliveryAddresses,
                    BillingAddressId = customer.Contacts.FirstOrDefault().BillingAddresses?.FirstOrDefault().Id.ToString(),
                    OrderName = recurringOrder.Name,
                    StartDate = recurringOrder.StartDate,
                    TimeSlotId = recurringOrder.TimeslotId.ToString(),
                    DeliveryAddressId = recurringOrder.DeliveryAddressId.ToString(),
                    FreeOrder = recurringOrder.IsFree,
                    OrderItems = orderItems,
                    TotalPrice = recurringOrder.TotalItemPrice.ToString(),
                    Reccurence = recurringOrder.WeeklyRecurrence.ToString(),
                    CompanyName = customer.CompanyName,
                    CustomerShortId = customer.ShortId
                };


                return View("EditOrder", order);
            }

            return View("EditOrder", null);
        }

        [HttpPost("/orders/{customerId}/{orderId}/edit")]
        public async Task<IActionResult> UpdateOrderAsync(string customerId, string orderId, Models.Orders.Order order)
        {
            if (!ModelState.IsValid)
            {
                var customer = await _fedWebClient.GetCustomerFullInfoAsync(customerId);
                order.CompanyName = customer.CompanyName;
                order.CustomerShortId = customer.ShortId;
                order.DeliveryAddress = customer.Contacts.SelectMany(c => c.DeliveryAddresses).ToList();
                var hubs = await _fedWebClient.GetHubsAsync();
                order.TimeSlots = await _fedWebClient.GetTimeslotsAsync(hubs.FirstOrDefault().Id);
                order.Products = await _fedWebClient.GetProductsAsync();
                order.Reccurence = ((Core.Enums.WeeklyRecurrence)(int.Parse(order.Reccurence))).ToString();


                return View("EditOrder", order);
            }
            var orderItems = new List<RecurringOrderItem>();
            foreach (var orderItem in order.OrderItems)
            {
                orderItems.Add(new RecurringOrderItem()
                {
                    ProductId = orderItem.ProductId,
                    ProductCode = orderItem.ProductCode,
                    Quantity = int.Parse(orderItem.Quantity),
                    AddedDate = DateTime.Now
                });
            }
            var endDate = (Core.Enums.WeeklyRecurrence)(int.Parse(order.Reccurence)) == Core.Enums.WeeklyRecurrence.EveryWeek ? DateTime.Now.AddYears(100) : DateTime.Parse(order.StartDate);

            var recurringOrder = new RecurringOrder(Guid.Parse(orderId), order.OrderName,
                Guid.Parse(order.ContactId),
                Guid.Parse(order.DeliveryAddressId),
                Guid.Parse(order.BillingAddressId),
                DateTime.Parse(order.StartDate),
               endDate,
                (Core.Enums.WeeklyRecurrence)(int.Parse(order.Reccurence)),
                 Guid.Parse(order.TimeSlotId),
                orderItems: orderItems, isFree: order.FreeOrder);


            await _fedWebClient.UpdateRecurringOrderAsync(Guid.Parse(orderId), recurringOrder);
            ViewBag.Success = "1";
            return RedirectToAction("DisplayCustomer", "Customers", new { customerId = customerId });
        }
    }
}
