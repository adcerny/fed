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
    public class DiscountService : IDiscountService
    {
        private readonly IDiscountsHandler _discountHandler;
        private readonly ICustomersHandler _customersHandler;
        private readonly IOrdersHandler _ordersHandler;
        private readonly IProductsHandler _productsHandler;
        private readonly IDiscountStrategyFactory _discountStrategyFactory;


        public DiscountService(IDiscountsHandler discountHandler,
                               ICustomersHandler customersHandler,
                               IOrdersHandler ordersHandler,
                               IDiscountStrategyFactory discountStrategyFactory,
                               IProductsHandler productsHandler)
        {
            _discountHandler = discountHandler ?? throw new ArgumentNullException(nameof(discountHandler));
            _customersHandler = customersHandler ?? throw new ArgumentNullException(nameof(customersHandler));
            _ordersHandler = ordersHandler ?? throw new ArgumentNullException(nameof(ordersHandler));
            _discountStrategyFactory = discountStrategyFactory ?? throw new ArgumentNullException(nameof(discountStrategyFactory));
            _productsHandler = productsHandler ?? throw new ArgumentNullException(nameof(productsHandler));
        }

        public async Task<Discount> GetDiscount(Guid discountId)
        {
            var discount = await _discountHandler.ExecuteAsync(new GetByIdQuery<Discount>(discountId));

            return discount;
        }

        public async Task<IList<Discount>> GetDiscounts(IList<Guid> discountIds)
        {
            var ids = discountIds.Select(id => id.ToString()).ToList();
            var discounts = await _discountHandler.ExecuteAsync(new GetByIdsQuery<Discount>(ids));

            return discounts;
        }

        public async Task<IList<Discount>> GetDiscounts(Guid? customerId = null, bool includeUnapplied = false)
        {
            if (customerId == null)
                return await _discountHandler.ExecuteAsync(new GetAllQuery<Discount>());
            else
            {
                var customer = await _customersHandler.ExecuteAsync(new GetByIdQuery<Customer>(customerId.Value));
                var discounts = await _discountHandler.ExecuteAsync(new GetDiscountsByCustomerQuery(customerId.Value, includeUnapplied));
                //TODO - pass in delivery date
                return discounts?.Where(d => IsDisountApplicableToOrder(d, customer, Date.Today))?.Select(d => d.Discount).ToList();
            }
        }

        public async Task<IList<DiscountResult>> CalculateDiscount(DiscountCalculationQuery query)
        {
            var results = new List<DiscountResult>();
            var discounts = new List<Discount>();
            
            if(query.CustomerId.HasValue)    
                discounts = (await GetDiscounts(query.CustomerId, true))?.ToList();
            else if(query.AppliedDiscountIds.OrEmptyIfNull().Count() > 0)
                discounts = (await GetDiscounts(query.AppliedDiscountIds.Distinct().ToList()))?.ToList();

            foreach (var discount in discounts.OrEmptyIfNull())
            {
                var result = _discountStrategyFactory.GetCalculator(discount).CalculateDiscount(query.OrderItems);
                results.Add(result);
            }

            return results;
        }

        public async Task<Discount> CreateDiscount(Discount discount)
        {
            if (discount.Percentage.HasValue && discount.Value.HasValue)
                throw new ArgumentNullException("Discounts cannot have both a percentage and a value");

            return await _discountHandler.ExecuteAsync(new CreateCommand<Discount>(discount));
        }

        public async Task<Discount> UpdateDiscount(Guid Id, Discount discount)
        {
            var result = await _discountHandler.ExecuteAsync(new UpdateCommand<Discount>(Id, discount));
            return result;
        }

        public async Task<bool> ApplyDiscount(Guid discountId, Guid customerId, string code = null)
        {
            var command = new CreateCommand<CustomerDiscount>(
                new CustomerDiscount
                {
                    CustomerId = customerId,
                    DiscountId = discountId,
                    AppliedDate = DateTime.Now,
                    DiscountCode = code
                }
            );

            return await _discountHandler.ExecuteAsync(command);
        }

        public async Task<bool> AddDiscountCode(Guid discountId, DiscountCode code)
        {
            var discount = await GetDiscountByCode(code.Code);
            if (discount != null)
                throw new ArgumentException($"A code with the code \"{code.Code}\" already exists");

            var codeId = Guid.NewGuid();

            var command = new CreateCommand<DiscountCode>(
                new DiscountCode(codeId, discountId, code.Code, code.Description, code.IsInactive)
            );

            return await _discountHandler.ExecuteAsync(command);
        }

        public async Task<Discount> GetDiscountByCode(string code)
        {
            return await _discountHandler.ExecuteAsync(new GetDiscountByCodeQuery(code));
        }

        public async Task<IList<Discount>> ApplyDiscounts(Guid customerId, DiscountEvent appliedEvent)
        {

            var customerDiscounts = await _discountHandler.ExecuteAsync(new GetDiscountsByCustomerQuery(customerId));

            var query = new GetDiscountsApplicableQuery(appliedEvent);
            var discounts = await _discountHandler.ExecuteAsync(query);

            //filter out any applied discounts, and any discounts after end date of event
            //and do not allow exclusive discounts if exclusive discounts are already applied
            var applicableDiscounts = discounts?
                .Where(d => !customerDiscounts.OrEmptyIfNull().Any(cd => cd.Discount.Id == d.Id) &&
                            (d.StartEventEndDate == null || d.StartEventEndDate > DateTime.Now) &&
                            (!customerDiscounts.OrEmptyIfNull().Where(d => d.Discount.IsExclusive).Any() && d.IsExclusive))
                      .ToList();

            foreach (var discount in applicableDiscounts.OrEmptyIfNull())
            {                
                var command = new CreateCommand<CustomerDiscount>(
                    new CustomerDiscount
                    {
                        CustomerId = customerId,
                        DiscountId = discount.Id,
                        AppliedDate = DateTime.Now
                    }
                );

                await _discountHandler.ExecuteAsync(command);
            }

            return applicableDiscounts;
        }

        public async Task<Discount> ApplyDiscountCode(string discountCode, Guid? customerId)
        {
            var discount = await GetDiscountByCode(discountCode);
            if (discount == null)
                throw new KeyNotFoundException($"Discount code {discountCode} not recognised");

            var customer = customerId.HasValue ? await _customersHandler.ExecuteAsync(new GetByIdQuery<Customer>(customerId.Value)) : null;

            var isApplicable = IsDiscountApplicableToCustomer(discount, customer);
            if (!isApplicable.Allowed)
                throw new ArgumentException(isApplicable.Message);

            if (customer != null)
            {
                var discounts = await _discountHandler.ExecuteAsync(new GetDiscountsByCustomerQuery(customerId.Value));
                if (discounts.OrEmptyIfNull().Any(d => d.Discount.Id == discount.Id))
                    throw new ArgumentException($"{discountCode} is already applied to this account");
                if (discount.IsExclusive && discounts.OrEmptyIfNull().Any(d => d.Discount.IsExclusive))
                    throw new ArgumentException("This discount cannot be used in combination with your other discount");

                await ApplyDiscount(discount.Id, customer.Id, discountCode);
            }
            return discount;
        }

        public async Task<IList<Discount>> GetDiscountsForDeliveryDate(Customer customer, Date deliveryDate)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            var customerDiscounts = await _discountHandler.ExecuteAsync(new GetDiscountsByCustomerQuery(customer.Id));

            var applicableDiscounts = new List<Discount>();

            foreach (var discount in customerDiscounts.OrEmptyIfNull())
            {
                if (IsDisountApplicableToOrder(discount, customer, deliveryDate))
                {
                    applicableDiscounts.Add(discount.Discount);
                }
            }
            return applicableDiscounts;
        }

        public async Task<decimal> GetRecurringOrderTotalDeduction(Customer customer, RecurringOrder recurringOrder, Date deliveryDate)
        {
            decimal total = 0;
            var disounts = await GetDiscountsForDeliveryDate(customer, deliveryDate);
            foreach (var discount in disounts.OrEmptyIfNull())
            {
                var result = _discountStrategyFactory.GetCalculator(discount).CalculateDiscount(recurringOrder.OrderItems.Select(i => new LineItem(i)).ToList());
                total += result.DiscountAmount;
            }
            return total;

        }

        public async Task<List<DiscountResult>> CreateOrderDiscounts(Order order)
        {
            var discounts = await GetDiscountsForDeliveryDate(await _customersHandler.ExecuteAsync(new GetByIdQuery<Customer>(order.CustomerId)), order.DeliveryDate);
            var discountResults = new List<DiscountResult>();

            foreach (var discount in discounts)
            {
                var calc = _discountStrategyFactory.GetCalculator(discount);

                var result = calc.CalculateDiscount(order.OrderItems.Select(i => new LineItem(i)).ToList());

                if (!result.DiscountQualification.IsQualified)
                    continue;

                discountResults.Add(result);
                var eligableOrderItems  = order.OrderItems.Where(i => result.EligibleProducts.Any(c => i.ProductCode == c.ProductCode)).ToList();

                decimal deduction = result.DiscountAmount;


                var od = new OrderDiscount
                {
                    OrderId = order.Id,
                    DiscountId = discount.Id,
                    Name = discount.Name,
                    Description = discount.Description,
                    Percentage = discount.Percentage,
                    Value = discount.Value,
                    OrderTotalDeduction = deduction
                };

                var command = new CreateCommand<OrderDiscount>(od);
                await _ordersHandler.ExecuteAsync(command);
                

                //if this discount has a percentage, record this against eligible items so that refunds / credit notes can be calculated
                foreach (var item in eligableOrderItems.OrEmptyIfNull())
                    await UpdateRefundableAmount(item, discount);         
            }

            return discountResults;
        }

        

        private bool IsDisountApplicableToOrder(CustomerDiscount customerDiscount, Customer customer, Date deliveryDate)
        {
            if (customerDiscount.Discount.PeriodFromStartDays == null)
                return true;

            if (customerDiscount.Discount.StartEvent == DiscountEvent.NextOrder)
                return true;

            if (customerDiscount.Discount.StartEvent == DiscountEvent.FirstOrder && customer.FirstDeliveryDate == null)
                return deliveryDate.Value < DateTime.Now.AddDays(customerDiscount.Discount.PeriodFromStartDays.Value);

            if (customerDiscount.Discount.StartEvent == DiscountEvent.FirstOrder)
                return deliveryDate.Value < (customer.FirstDeliveryDate ?? DateTime.Today).AddDays(customerDiscount.Discount.PeriodFromStartDays.Value);

            if (customerDiscount.Discount.StartEvent == DiscountEvent.FirstOrder)
                return deliveryDate.Value < (customer.FirstDeliveryDate ?? DateTime.Today).AddDays(customerDiscount.Discount.PeriodFromStartDays.Value);

            if (customerDiscount.Discount.StartEvent == DiscountEvent.CodeEntered || 
                customerDiscount.Discount.StartEvent == DiscountEvent.Manual)
                return deliveryDate.Value < (customerDiscount.AppliedDate ?? DateTime.Today).AddDays(customerDiscount.Discount.PeriodFromStartDays.Value);

            //TODO - implement future logic for other start events
            return false;
        }

        private DiscountCodeResult IsDiscountApplicableToCustomer(Discount discount, Customer customer)
        {
            if (discount.StartEvent == DiscountEvent.FirstOrder &&
                customer != null &&
                customer.LastDeliveryDate != null)
                return new DiscountCodeResult { Allowed = false, Message = "This discount is for new customers" };

            //TODO - implement future logic for other start events
            return new DiscountCodeResult { Allowed = true }; ;
        }

        private async Task<bool> UpdateRefundableAmount(OrderItem item, Discount discount)
        {
            item.RefundablePrice = item.ActualPrice - discount.GetPercentageDiscount(item.ActualPrice);
            await _ordersHandler.ExecuteAsync(new UpdateOrderItemCommand(item.OrderId, item.ProductId, item));
            return true;
        }

        private class DiscountCodeResult
        {
            public bool Allowed { get; set; }
            public string Message { get; set; }
        }
    }
}
