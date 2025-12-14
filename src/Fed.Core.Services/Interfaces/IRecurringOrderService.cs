using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IRecurringOrderService
    {
        Task<RecurringOrder> GetRecurringOrderAsync(Guid id);
        Task<IList<RecurringOrder>> GetRecurringOrdersAsync(Guid contactId, DateRange dateRange, bool includeFromDemoAccounts);

        Task<RecurringOrder> CreateAsync(RecurringOrder recurringOrder);

        Task<RecurringOrder> UpdateAsync(Guid recurringOrderId, RecurringOrder recurringOrder);
        Task<RecurringOrder> UpdateForSingleDateAsync(Guid recurringOrderId, Date deliveryDate, RecurringOrder recurringOrder);
        Task<RecurringOrder> UpdateFromDateAsync(Guid recurringOrderId, Date deliveryDate, RecurringOrder recurringOrder);
        Task<OrderDeliveryContext<RecurringOrder>> GetRecurringOrderForDateAsync(Guid recurringOrderId, Date deliveryDate);
        Task<DeliveryChargeResult> GetDeliveryChargeForTimeslotDateAsync(Guid customerId, Guid timeslotId, Guid? recurringOrderId, Date? deliveryDate);

        Task DeleteAsync(Guid id);
    }
}