using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<Discount> GetDiscount(Guid discountId);

        Task<IList<Discount>> GetDiscounts(IList<Guid> discountIds);

        Task<IList<Discount>> GetDiscounts(Guid? customerId = null, bool includeUnapplied = false);

        Task<IList<DiscountResult>> CalculateDiscount(DiscountCalculationQuery query);

        Task<Discount> CreateDiscount(Discount discount);

        Task<Discount> UpdateDiscount(Guid id, Discount discount);

        Task<IList<Discount>> ApplyDiscounts(Guid customerId, DiscountEvent appliedEvent);

        Task<bool> ApplyDiscount(Guid discountId, Guid customerId, string code = null);

        Task<IList<Discount>> GetDiscountsForDeliveryDate(Customer customer, Date deliveryDate);

        Task<List<DiscountResult>> CreateOrderDiscounts(Order order);

        Task<decimal> GetRecurringOrderTotalDeduction(Customer customer, RecurringOrder recurringOrder, Date deliveryDate);

        Task<bool> AddDiscountCode(Guid discountId, DiscountCode code);

        Task<Discount> GetDiscountByCode(string code);

        Task<Discount> ApplyDiscountCode(string discountCode, Guid? customerId);
    }
}
