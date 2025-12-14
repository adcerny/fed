using Fed.Core.Common;
using Fed.Core.Common.Interfaces;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly DateTime InceptionDate = new DateTime(2019, 1, 1);

        private readonly IForecastService _forecastService;
        private readonly IOrdersHandler _ordersHandler;
        private readonly IProductsHandler _productsHandler;
        private readonly IOrderIdGenerator _orderIdGenerator;
        private readonly ITimeslotsService _timeslotsService;
        private readonly ICustomersHandler _customersHandler;
        private readonly IDiscountService _discountService;

        public OrderService(
            IForecastService forecastService,
            IOrdersHandler ordersHandler,
            IProductsHandler productsHandler,
            IOrderIdGenerator orderIdGenerator,
            ITimeslotsService timeslotHandler,
            ICustomersHandler customersHandler,
            IDiscountService discountService)
        {
            _forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
            _ordersHandler = ordersHandler ?? throw new ArgumentNullException(nameof(ordersHandler));
            _productsHandler = productsHandler ?? throw new ArgumentNullException(nameof(productsHandler));
            _orderIdGenerator = orderIdGenerator ?? throw new ArgumentNullException(nameof(orderIdGenerator));
            _timeslotsService = timeslotHandler ?? throw new ArgumentNullException(nameof(timeslotHandler));
            _customersHandler = customersHandler ?? throw new ArgumentNullException(nameof(customersHandler));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        }

        public async Task<IList<GeneratedOrder>> GenerateOrdersAsync(Date date)
        {
            // 1. Return generated orders if they already exist:

            var existingOrders = await _ordersHandler.ExecuteAsync(new GetOrdersQuery(DateRange.SingleDay(date)));

            if (existingOrders != null && existingOrders.Count > 0)
                return
                    existingOrders
                        .Select(o => new GeneratedOrder(o.RecurringOrderId, o.Id, o.ShortId))
                        .ToList();

            // 2. Otherwise get a forecast and generate new orders:

            var forecast = await _forecastService.GetForecastAsync(DateRange.SingleDay(date));
            var generatedOrders = new List<GeneratedOrder>();

            if (forecast.ContainsKey(date))
            {
                var recurringOrders = forecast[date];
                var i = 0;

                foreach (var recurringOrder in recurringOrders)
                {
                    var shortId = _orderIdGenerator.GenerateId(date.Value, i);
                    var command = new CreateOrderFromRecurringOrderCommand(recurringOrder.Id, shortId, date, recurringOrder.IsFree);
                    var orderId = await _ordersHandler.ExecuteAsync(command);
                    generatedOrders.Add(
                        new GeneratedOrder(
                            recurringOrder.Id,
                            orderId,
                            shortId));

                    var order = await _ordersHandler.ExecuteAsync(new GetByIdQuery<Order>(orderId));
                    var results = await _discountService.CreateOrderDiscounts(order);

                    //add any discounted products to separate one off order
                    i++;
                    foreach (var result in results.Where(r => r.DiscountedProducts.OrEmptyIfNull().Count() > 0))
                    {
                        await CreateDiscounteOrder(date, i, order, result);
                        i++;
                    }              
                }
            }

            return generatedOrders;
        }

        private async Task CreateDiscounteOrder(Date date, int i, Order order, DiscountResult result)
        {
            var dicountedItems = new List<OrderItem>();
            
            foreach (var discountedProduct in result.DiscountedProducts.OrEmptyIfNull())
            {

                var item = new OrderItem
                {
                    ProductCode = discountedProduct.ProductCode,
                    Quantity = discountedProduct.Quantity,
                    SalePrice = discountedProduct.Price,
                    RefundablePrice = discountedProduct.Price
                };
                dicountedItems.Add(item);
            }

            var sid = _orderIdGenerator.GenerateId(date.Value, i);
            var discountOrder = new Order
            {
                OrderName = result.DiscountName,
                TimeslotId = order.TimeslotId,
                ContactId = order.ContactId,
                WeeklyRecurrence = order.WeeklyRecurrence,
                ShortId = sid,
                DeliveryDate = order.DeliveryDate,
                IsFree = false,
                OrderItems = dicountedItems
            };

            var id = await _ordersHandler.ExecuteAsync(new CreateCommand<Order>(discountOrder));

            await _ordersHandler.ExecuteAsync(new UpdateOrderDiscountCommand(order.Id, result.DiscountId, id));
        }

        public async Task<IList<Order>> GetOrdersAsync(Date date, Guid? contactId, bool excludeUnpaid)
        {
            var orders = await _ordersHandler.ExecuteAsync(new GetOrdersQuery(DateRange.SingleDay(date), contactId, excludeUnpaid));

            return orders;
        }

        public async Task<Order> GetOrderAsync(Guid id)
        {
            var query = new GetByIdQuery<Order>(id);
            var order = await _ordersHandler.ExecuteAsync(query);

            return order;
        }

        public async Task<IList<OrderSummary>> GetOrderSummaryAsync(Date fromDate, Date toDate, Guid contactId)
        {
            var dateRange = DateRange.Create(fromDate, toDate);

            var orderSummary = new List<OrderSummary>();

            var customer = await _customersHandler.ExecuteAsync(new GetCustomerByContactIdQuery(contactId));

            orderSummary.AddRange(await GetConfirmedOrderSummary(contactId, dateRange));
            orderSummary.AddRange(await GetForcastedOrderSummary(customer, contactId, dateRange));

            //check if we need to apply delivery charge

            if (!customer.IsDeliveryChargeExempt)
                await ApplyDeliveryCharge(orderSummary);

            return orderSummary.OrderBy(o => o.DeliveryDate)
                               .ThenBy(o => o.EarliestTime)
                               .ThenByDescending(o => o.WeeklyRecurrence)
                               .ThenBy(o => o.DateAdded).ToList();
        }

        private async Task<List<OrderSummary>> GetForcastedOrderSummary(Customer customer, Guid contactId, DateRange dateRange)
        {
            var orderSummary = new List<OrderSummary>();

            var tomorrow = Date.Today.AddDays(1);
            var forecastFromDate = tomorrow < dateRange.From
                ? dateRange.From
                : tomorrow;

            var forecastDateRange = DateRange.Create(forecastFromDate, dateRange.To);

            var forecast = await _forecastService.GetForecastAsync(forecastDateRange, contactId, true, true);

            var products = await _productsHandler.ExecuteAsync(new GetProductsQuery(includeDeleted: true));

            foreach (var date in forecast.Keys)
            {
                foreach (var recurringOrder in forecast[date])
                {
                    if (recurringOrder.IsFree)
                        continue;

                    var categoryIcons = new Dictionary<string, CategoryIcon>();

                    foreach (var item in recurringOrder.OrderItems)
                    {
                        var product = products.SingleOrDefault(p => p.Id.Equals(item.ProductId, StringComparison.OrdinalIgnoreCase));

                        if (product != default(Product))
                        {
                            if (categoryIcons.ContainsKey(product.IconCategory))
                            {
                                categoryIcons[product.IconCategory].LineItemCount++;
                                categoryIcons[product.IconCategory].TotalQuantity += item.Quantity;
                            }
                            else
                            {
                                categoryIcons.Add(
                                    product.IconCategory,
                                    new CategoryIcon
                                    {
                                        Icon = product.IconCategory,
                                        LineItemCount = 1,
                                        TotalQuantity = item.Quantity
                                    });
                            }
                        }
                    }

                    var totalDiscount = await _discountService.GetRecurringOrderTotalDeduction(customer, recurringOrder, date);

                    var upcomingOrder =
                        new OrderSummary(
                            recurringOrder.Id,
                            date.Value,
                            recurringOrder.Name,
                            recurringOrder.TimeslotId,
                            recurringOrder.Timeslot.EarliestTime,
                            recurringOrder.Timeslot.LatestTime,
                            recurringOrder.TotalItemPrice - totalDiscount,
                            recurringOrder.WeeklyRecurrence,
                            recurringOrder.CreatedDate,
                            categoryIcons.Values.ToList(),
                            $"{recurringOrder.DeliveryAddress.AddressLine1}, {recurringOrder.DeliveryAddress.Postcode}")
                        {
                            Status = OrderStatus.Unconfirmed
                        };
                    orderSummary.Add(upcomingOrder);
                }
            }
            return orderSummary;
        }

        private async Task<List<OrderSummary>> GetConfirmedOrderSummary(Guid contactId, DateRange dateRange)
        {
            var orderSummary = new List<OrderSummary>();
            var query = new GetOrderSummaryQuery(dateRange, contactId);

            var confirmedOrderSummary = await _ordersHandler.ExecuteAsync(query);

            if (confirmedOrderSummary != null)
            {
                foreach (var order in confirmedOrderSummary)
                {
                    var now = DateTime.Now.ToBritishTime();
                    var earliestTime = order.DeliveryDate.Date.Add(order.EarliestTime);
                    var latestTime = order.DeliveryDate.Date.Add(order.LatestTime);

                    if (now >= latestTime)
                        order.Status = OrderStatus.Delivered;

                    else if (earliestTime <= now &&
                             latestTime > now)

                        order.Status = OrderStatus.Despatched;
                    else
                        order.Status = OrderStatus.Confirmed;
                }

                orderSummary.AddRange(confirmedOrderSummary);
            }

            return orderSummary;
        }

        private async Task ApplyDeliveryCharge(List<OrderSummary> orderSummary)
        {
            var chargeableOrders = orderSummary.GroupBy(o => new { o.DeliveryDate, o.TimeslotId })
                                               .SelectMany(g => g.OrderByDescending(s => s.WeeklyRecurrence)
                                               .ThenBy(s => s.DateAdded).Take(1)).ToList();

            var timeslots = await _timeslotsService.GetTimeslots(Guid.Empty, false, true);

            foreach (var order in chargeableOrders)
            {
                var deliveryCharge = timeslots.Where(t => t.Id == order.TimeslotId).FirstOrDefault().DeliveryCharge;
                order.TotalPrice = order.TotalPrice + deliveryCharge;
            }
        }

        public async Task<OrderDeliveryContext<Order>> GetOrderForDateAsync(Guid orderId, Date deliveryDate)
        {
            var order = await GetOrderAsync(orderId);

            if (order == null)
                return null;

            var customer = await _customersHandler.ExecuteAsync(new GetCustomerByContactIdQuery(order.ContactId));

            var orderForDate = new OrderDeliveryContext<Order>
            {
                Order = order,
                IsDeliveryChargeExempt = customer.IsDeliveryChargeExempt
            };
            foreach (var d in order.OrderDiscounts.OrEmptyIfNull())
            {
                orderForDate.Discounts?.Add(await _discountService.GetDiscount(d.DiscountId));
            }

            if (!customer.IsDeliveryChargeExempt)
            {
                var orders = await _ordersHandler.ExecuteAsync(new GetOrdersQuery(DateRange.SingleDay(deliveryDate), order.ContactId, false));

                var chargeableorder = orders?.Where(o => o.TimeslotId == order.TimeslotId).OrEmptyIfNull()
                                   .OrderByDescending(o => o.WeeklyRecurrence)
                                   .ThenBy(o => o.OrderGeneratedDate)
                                   .FirstOrDefault();

                var timeslots = await _timeslotsService.GetTimeslots(Guid.Empty, false, true);

                orderForDate.DeliveryCharge = timeslots.Where(t => t.Id == chargeableorder.TimeslotId).Single().DeliveryCharge;
                orderForDate.DeliveryChargeableRecurringOrderId = chargeableorder.Id;
            }

            return orderForDate;
        }
    }
}
