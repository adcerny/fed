using Fed.Core.Common;
using Fed.Core.Common.Interfaces;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Exceptions;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IOrderService _orderService;
        private readonly IDeliveryIdGenerator _deliveryIdGenerator;
        private readonly IDeliveriesHandler _deliveriesHandler;
        private readonly IDeliveryShortageHandler _deliveryShortageHandler;
        private readonly IDeliveryAdditionHandler _deliveryAdditionHandler;
        private readonly ITimeslotsService _timeslotsService;
        private readonly ICustomersHandler _customersHandler;
        private readonly IProductsHandler _productsHandler;

        public DeliveryService(
            IOrderService orderService,
            IDeliveryIdGenerator deliveryIdGenerator,
            IDeliveriesHandler deliveriesHandler,
            IDeliveryShortageHandler deliveryShortageHandler,
            IDeliveryAdditionHandler deliveryAdditionHandler,
            ITimeslotsService timeslotsHandler,
            ICustomersHandler customersHandler,
            IProductsHandler productsHandler)
        {
            _orderService = orderService;
            _deliveryIdGenerator = deliveryIdGenerator;
            _deliveriesHandler = deliveriesHandler;
            _deliveryShortageHandler = deliveryShortageHandler;
            _deliveryAdditionHandler = deliveryAdditionHandler;
            _timeslotsService = timeslotsHandler;
            _customersHandler = customersHandler;
            _productsHandler = productsHandler;
        }

        // Deliveries

        public async Task<IList<Delivery>> GetDeliveriesAsync(Date date)
        {
            var deliveries = await _deliveriesHandler.ExecuteAsync(new GetDeliveriesQuery(DateRange.SingleDay(date)));

            return deliveries.Where(d => d.Orders.Any(o => o.HasOrderItems())).ToList();
        }

        public async Task<Delivery> GetDeliveryAsync(string id)
        {
            var query = new GetByIdQuery<Delivery>(id);
            var order = await _deliveriesHandler.ExecuteAsync(query);

            return order;
        }

        public async Task<IList<Delivery>> CreateDeliveriesAsync(Date date)
        {
            // 1. Return already generated deliveries if they exist:

            var existingDeliveries = await _deliveriesHandler.ExecuteAsync(new GetDeliveriesQuery(DateRange.SingleDay(date)));

            if (existingDeliveries != null && existingDeliveries.Count > 0)
                return existingDeliveries;

            // 2. Otherwise generate deliveries:

            var deliveries = new List<Delivery>();

            var orders = await _orderService.GetOrdersAsync(date, null, false);
            var ordersGroupedByTimeslot = orders.GroupBy(o => o.TimeslotId);

            var timeslots = await _timeslotsService.GetTimeslots(Guid.Empty, false, true);


            foreach (var ordersByTimeslot in ordersGroupedByTimeslot)
            {
                var timeslotBatch = ordersByTimeslot.ToList();
                var sampleTimeslotOrder = timeslotBatch.First();

                var timeslotId = ordersByTimeslot.Key;
                var timeslot = timeslots.Where(t => t.Id == timeslotId).FirstOrDefault();
                var earliestTime = sampleTimeslotOrder.EarliestTime;
                var latestTime = sampleTimeslotOrder.LatestTime;

                var ordersGroupedByContact =
                    timeslotBatch
                        .GroupBy(o => new { o.ContactId, DeliveryGroup = (o.SplitDeliveriesByOrder ? o.Id : o.DeliveryAddressId) })
                        .OrderBy(g => g.ToList().First().CompanyName);

                var i = 0;

                foreach (var ordersByContact in ordersGroupedByContact)
                {
                    var deliveryBatch = ordersByContact.ToList();
                    var customer = await _customersHandler.ExecuteAsync(new GetCustomerByContactIdQuery(ordersByContact.Key.ContactId));

                    // If a delivery doesn't have a single order with order items
                    // then don't generate a delivery:
                    if (!deliveryBatch.Any(o => o.OrderItems is object && o.OrderItems.Count > 0))
                        continue;

                    var sampleDeliveryOrder = deliveryBatch.First();

                    var contactId = ordersByContact.Key.ContactId;
                    var deliveryAddressId = sampleDeliveryOrder.DeliveryAddressId;
                    var companyName = sampleDeliveryOrder.DeliveryCompanyName;
                    var fullName = sampleDeliveryOrder.DeliveryFullName;
                    var addressLine1 = sampleDeliveryOrder.DeliveryAddressLine1;
                    var addressLine2 = sampleDeliveryOrder.DeliveryAddressLine2;
                    var town = sampleDeliveryOrder.DeliveryTown;
                    var postcode = sampleDeliveryOrder.DeliveryPostcode;
                    var deliveryInstructions = sampleDeliveryOrder.DeliveryInstructions;
                    var leaveOutside = sampleDeliveryOrder.LeaveDeliveryOutside;

                    // Internal, Presale and customers who are exempt from the delivery charge,
                    // should have a delivery charge of zero (Rules set by Chris in "verbally" via Teams)
                    var deliveryCharge =
                        customer.IsDeliveryChargeExempt
                        || customer.AccountType == AccountType.Internal
                        || customer.AccountType == AccountType.Presale
                        //if all orders are free, don't apply delivery charge
                        || deliveryBatch.All(b => b.IsFree)
                        //if we are splitting deliveries by order, only one delivery to an address should have delivery charge
                        || (deliveryBatch.All(b => b.SplitDeliveriesByOrder) && deliveries.Where(d => d.DeliveryAddressId == deliveryAddressId && d.DeliveryCharge > 0).Any())
                        ? 0
                        : timeslot.DeliveryCharge;

                    var shortId = _deliveryIdGenerator.GenerateId(date.Value, latestTime.Hours, i);

                    var delivery = new Delivery
                    {
                        Id = Guid.NewGuid(),
                        ShortId = shortId,
                        ContactId = contactId,
                        DeliveryAddressId = deliveryAddressId,
                        DeliveryDate = date,
                        TimeslotId = timeslotId,
                        EarliestTime = earliestTime,
                        LatestTime = latestTime,
                        DeliveryCharge = deliveryCharge,
                        DeliveryCompanyName = companyName,
                        DeliveryFullName = fullName,
                        DeliveryAddressLine1 = addressLine1,
                        DeliveryAddressLine2 = addressLine2,
                        DeliveryTown = town,
                        DeliveryPostcode = postcode,
                        DeliveryInstructions = deliveryInstructions,
                        LeaveDeliveryOutside = leaveOutside,

                        Orders = deliveryBatch
                    };

                    deliveries.Add(delivery);

                    i++;
                }
            }

            // 3. Save deliveries in database
            var createdDeliveries = await _deliveriesHandler.ExecuteAsync(new CreateCommand<IList<Delivery>>(deliveries));

            return createdDeliveries;
        }

        public async Task<bool> DeleteDeliveriesAsync(Date deliveryDate)
        {
            return await _deliveriesHandler.ExecuteAsync(new DeleteDeliveryCommand(deliveryDate));
        }

        public async Task<bool> SetDeliveryPackagingStatusAsync(Guid deliveryId, PackingStatus packagingStatus)
        {
            return await _deliveriesHandler.ExecuteAsync(new SetDeliveryPackingStatusCommand(deliveryId, packagingStatus));
        }

        public async Task<bool> SetDeliveryBagCountAsync(string deliveryId, int bagCount)
        {
            return await _deliveriesHandler.ExecuteAsync(new SetDeliveryBagCountCommand(deliveryId, bagCount));
        }

        // Delivery Shortages

        public async Task<DeliveryShortage> ShortDeliveryItemAsync(
        Guid orderId,
        string productId,
        int actualQuantity,
        decimal productPrice,
        string reason,
        string reasonCode)
        {
            var order = await _orderService.GetOrderAsync(orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            if (!order.OrderItems.Any(x => x.ProductId.Equals(productId, StringComparison.OrdinalIgnoreCase)))
                throw new OrderDoesNotContainProductException(orderId, productId);

            var existingShortage = await _deliveryShortageHandler.ExecuteAsync(new GetDeliveryShortageQuery(orderId, productId));

            if (existingShortage != null)
                throw new ProductAlreadyShortenedException(orderId, productId);

            var orderItem = order.OrderItems.Single(x => x.ProductId.Equals(productId, StringComparison.OrdinalIgnoreCase));

            var desiredQuantity = orderItem.Quantity;

            if (actualQuantity >= desiredQuantity)
                throw new ShortenedQuantityMustBeLessThanOrderedQuantityException(desiredQuantity, actualQuantity);

            if (string.IsNullOrEmpty(reason))
                throw new MissingReasonForShortageException();

            var deliveryShortage =
                new DeliveryShortage(
                    Guid.NewGuid(),
                    orderId,
                    productId,
                    desiredQuantity,
                    actualQuantity,
                    reason,
                    reasonCode,
                    DateTime.Now.TimeOfDay,
                    productPrice);

            var result = await _deliveryShortageHandler.ExecuteAsync(new CreateCommand<DeliveryShortage>(deliveryShortage));

            return result;
        }

        public async Task DeleteDeliveryShortageAsync(Guid deliveryShortageId)
        {
            var deliveryShortage = await _deliveryShortageHandler.ExecuteAsync(new GetByIdQuery<DeliveryShortage>(deliveryShortageId));

            if (deliveryShortage == null)
                throw new DeliveryShortageDoesNotExistException(deliveryShortageId);

            await _deliveryShortageHandler.ExecuteAsync(new DeleteCommand<DeliveryShortage>(deliveryShortageId));
        }

        public Task<IList<DeliveryShortage>> GetDeliveryShortagesAsync(Date date) =>
            _deliveryShortageHandler.ExecuteAsync(new GetDeliveryShortagesQuery(DateRange.SingleDay(date)));

        // Delivery Additions

        public async Task<DeliveryAddition> AddSubstituteToDeliveryAsync(Guid orderId, string productId, int quantity, string notes, Guid deliveryShortageId)
        {
            var order = await _orderService.GetOrderAsync(orderId);

            if (order == null)
                throw new OrderNotFoundException(orderId);

            var existingAddition = await _deliveryAdditionHandler.ExecuteAsync(new GetDeliveryAdditionQuery(orderId, productId));

            if (existingAddition != null)
                throw new ProductSubstituteAlreadyAddedException(orderId, productId);

            var product = await _productsHandler.ExecuteAsync(new GetByIdQuery<Product>(productId));

            if (product == null)
                throw new ProductDoesNotExistException(productId);

            if (product.IsDeleted)
                throw new ProductHasBeenDiscontinuedException(productId);

            if (quantity <= 0)
                throw new QuantityCannotBeZeroException();

            var deliveryAddition =
                DeliveryAddition.CreateSubstitute(
                    orderId, productId, product.ProductName, product.ProductCode, product.ActualPrice, product.IsTaxable, quantity, notes, deliveryShortageId);

            var result = await _deliveryAdditionHandler.ExecuteAsync(new CreateCommand<DeliveryAddition>(deliveryAddition));

            return result;
        }

        public async Task DeleteDeliveryAdditionAsync(Guid deliveryAdditionId)
        {
            var deliveryAddition = await _deliveryAdditionHandler.ExecuteAsync(new GetByIdQuery<DeliveryAddition>(deliveryAdditionId));

            if (deliveryAddition == null)
                throw new DeliveryAdditionDoesNotExistException(deliveryAdditionId);

            await _deliveryAdditionHandler.ExecuteAsync(new DeleteCommand<DeliveryAddition>(deliveryAdditionId));
        }

        public Task<IList<DeliveryAddition>> GetDeliveryAdditionsAsync(Date date) =>
            _deliveryAdditionHandler.ExecuteAsync(new GetDeliveryAdditionsQuery(DateRange.SingleDay(date)));
    }
}