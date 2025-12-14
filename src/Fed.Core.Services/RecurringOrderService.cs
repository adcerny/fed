using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Exceptions;
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
    public class RecurringOrderService : IRecurringOrderService
    {
        private readonly IRecurringOrdersHandler _recurringOrdersHandler;
        private readonly ISkipDatesHandler _skipDatesHandler;
        private readonly ICustomersHandler _customersHandler;
        private readonly ITimeslotsService _timeslotsService;
        private readonly IForecastService _forecastService;
        private readonly IDiscountService _discountService;



        public RecurringOrderService(
            IRecurringOrdersHandler recurringOrdersHandler,
            ISkipDatesHandler skipDatesHandler,
            IForecastService forecastService,
            ICustomersHandler customersHandler,
            ITimeslotsService timeslotsHandler,
            IDiscountService discountService)
        {
            _recurringOrdersHandler =
                recurringOrdersHandler
                ?? throw new ArgumentNullException(nameof(recurringOrdersHandler));

            _skipDatesHandler =
                skipDatesHandler
                ?? throw new ArgumentNullException(nameof(skipDatesHandler));

            _forecastService =
                forecastService
                ?? throw new ArgumentNullException(nameof(forecastService));

            _customersHandler =
                customersHandler
                ?? throw new ArgumentNullException(nameof(customersHandler));

            _discountService =
                discountService
                ?? throw new ArgumentNullException(nameof(discountService));

            _timeslotsService =
                timeslotsHandler
                ?? throw new ArgumentNullException(nameof(timeslotsHandler));
        }

        public Task<RecurringOrder> GetRecurringOrderAsync(Guid id) =>
            _recurringOrdersHandler.ExecuteAsync(new GetByIdQuery<RecurringOrder>(id));

        public Task<IList<RecurringOrder>> GetRecurringOrdersAsync(
            Guid contactId,
            DateRange dateRange,
            bool includeFromDemoAccounts) =>
            _recurringOrdersHandler.ExecuteAsync(new GetRecurringOrdersQuery(dateRange, contactId, includeFromDemoAccounts: includeFromDemoAccounts));

        public async Task<RecurringOrder> CreateAsync(RecurringOrder recurringOrder)
        {
            var customer = await _customersHandler.ExecuteAsync(new GetCustomerByContactIdQuery(recurringOrder.ContactId));

            if (customer.AccountType == AccountType.Deleted)
                throw new InvalidOperationForDeletedAccountException();

            if (IsOneOffOrderForNextDayAfterCutOffTime(recurringOrder))
                throw new PastCutOffException();

            var timeslot = await _timeslotsService.GetTimeslot(recurringOrder.TimeslotId);

            //if we've missed the cut off for the next delivery date, set the start date the first possible week
            recurringOrder.StartDate = recurringOrder.StartDate.Value.NextWeekday(timeslot.DayOfWeek).GetNextAvailableDeliveryDate();

            var cmd = new CreateCommand<RecurringOrder>(recurringOrder);
            var result = await _recurringOrdersHandler.ExecuteAsync(cmd);

            return await AdjustDates(result);
        }

        public async Task<RecurringOrder> UpdateAsync(Guid recurringOrderId, RecurringOrder recurringOrder)
        {
            var customer = await _customersHandler.ExecuteAsync(new GetCustomerByContactIdQuery(recurringOrder.ContactId));

            if (customer.AccountType == AccountType.Deleted)
                throw new InvalidOperationForDeletedAccountException();

            var originalRecurringOrder = await GetRecurringOrderAsync(recurringOrderId);

            if (originalRecurringOrder == null)
                throw new OrderNotFoundException(recurringOrderId);

            if (IsOneOffOrderForNextDayAfterCutOffTime(recurringOrder))
                throw new PastCutOffException();

            var cmd = new UpdateCommand<RecurringOrder>(recurringOrder.Id, recurringOrder);
            var updatedOrder = await _recurringOrdersHandler.ExecuteAsync(cmd);

            updatedOrder = await AdjustDates(updatedOrder);
            return updatedOrder;
        }

        public async Task<RecurringOrder> UpdateForSingleDateAsync(
            Guid recurringOrderId, Date deliveryDate, RecurringOrder recurringOrder)
        {
            var customer = await _customersHandler.ExecuteAsync(new GetCustomerByContactIdQuery(recurringOrder.ContactId));

            if (customer.AccountType == AccountType.Deleted)
                throw new InvalidOperationForDeletedAccountException();

            var originalRecurringOrder = await GetRecurringOrderAsync(recurringOrderId);

            if (originalRecurringOrder == null)
                throw new OrderNotFoundException(recurringOrderId);

            //adjust dates
            recurringOrder.WeeklyRecurrence = WeeklyRecurrence.OneOff;
            recurringOrder.StartDate = deliveryDate;
            recurringOrder.EndDate = deliveryDate;

            if (IsOneOffOrderForNextDayAfterCutOffTime(recurringOrder))
                throw new PastCutOffException();

            if (originalRecurringOrder.WeeklyRecurrence == WeeklyRecurrence.OneOff)
            {
                var updatedOrder = await _recurringOrdersHandler.ExecuteAsync(new UpdateCommand<RecurringOrder>(recurringOrder.Id, recurringOrder));
                return updatedOrder;
            }

            // Skip the recurring order on the date provided
            var skipCmd = new CreateSkipDateCommand(
            recurringOrderId,
            deliveryDate,
            "Recurring order edited for one date only", "Website");

            await _skipDatesHandler.ExecuteAsync(skipCmd);

            var cmd = new CreateCommand<RecurringOrder>(recurringOrder);
            var newOrder = await _recurringOrdersHandler.ExecuteAsync(cmd);
            return await AdjustDates(newOrder);
        }

        public async Task<RecurringOrder> UpdateFromDateAsync(Guid recurringOrderId, Date deliveryDate, RecurringOrder recurringOrder)
        {
            var customer = await _customersHandler.ExecuteAsync(new GetCustomerByContactIdQuery(recurringOrder.ContactId));

            if (customer.AccountType == AccountType.Deleted)
                throw new InvalidOperationForDeletedAccountException();

            var originalRecurringOrder = await GetRecurringOrderAsync(recurringOrderId);

            if (originalRecurringOrder == null)
                throw new OrderNotFoundException(recurringOrderId);

            if (originalRecurringOrder.WeeklyRecurrence == WeeklyRecurrence.OneOff)
                throw new InvalidOperationForOneOffOrders();

            if (deliveryDate < Date.Today)
                throw new ArgumentException($"{deliveryDate} is in the past");

            if (deliveryDate > recurringOrder.EndDate)
                throw new ArgumentException($"{deliveryDate} is before the orders end date of {recurringOrder.EndDate}");

            var loadExistingRecurringOrderQuery = new GetByIdQuery<RecurringOrder>(recurringOrderId);
            var currentRecurringOrder = await _recurringOrdersHandler.ExecuteAsync(loadExistingRecurringOrderQuery);

            // If the chosen swap over date is the next delivery date then just apply a normal update:
            if (currentRecurringOrder.NextDeliveryDate >= deliveryDate)
            {
                var order = await _recurringOrdersHandler.ExecuteAsync(
                    new UpdateCommand<RecurringOrder>(recurringOrderId, recurringOrder));

                return await AdjustDates(order);
            }

            // Set the current recurring order to end a week before the modified recurring order:
            currentRecurringOrder.EndDate = deliveryDate.AddDays(-7);

            await _recurringOrdersHandler.ExecuteAsync(
                new UpdateCommand<RecurringOrder>(recurringOrderId, currentRecurringOrder));

            // Start the newly modified recurring order from the selected date:
            recurringOrder.StartDate = deliveryDate;
            recurringOrder.EndDate = Date.MaxDate;

            var updatedOrder = await _recurringOrdersHandler.ExecuteAsync(
                new CreateCommand<RecurringOrder>(recurringOrder));

            return await AdjustDates(updatedOrder);
        }

        public async Task DeleteAsync(Guid id)
        {
            var recurringOrder = await _recurringOrdersHandler.ExecuteAsync(new GetByIdQuery<RecurringOrder>(id));

            var customer = await _customersHandler.ExecuteAsync(new GetCustomerByContactIdQuery(recurringOrder.ContactId));

            if (customer.AccountType == AccountType.Deleted)
                throw new InvalidOperationForDeletedAccountException();

            bool result = await _recurringOrdersHandler.ExecuteAsync(new DeleteCommand<RecurringOrder>(id));

            if (!result)
                throw new OrderNotFoundException(id);
        }

        public bool IsOneOffOrderForNextDayAfterCutOffTime(RecurringOrder recurringOrder)
        {
            if (recurringOrder.WeeklyRecurrence != WeeklyRecurrence.OneOff)
                return false;

            if (recurringOrder.StartDate.Value <= DateTime.Today)
                return true;

            var nextWorkingDay = DateTime.Today.GetNextWorkingDay();

            if (recurringOrder.StartDate.Value > nextWorkingDay)
                return false;

            return DateTime.Now.IsAfterCutOffTime();

        }

        // Checks if start end dates fall on same day as timeslot
        private async Task<RecurringOrder> AdjustDates(RecurringOrder recurringOrder)
        {
            var timeslot = recurringOrder.Timeslot ?? await _timeslotsService.GetTimeslot(recurringOrder.TimeslotId);

            var timeslotDay = timeslot.DayOfWeek;

            if (recurringOrder.StartDate.Value.DayOfWeek == timeslotDay &&
               recurringOrder.EndDate.GetValueOrDefault().Value.DayOfWeek == timeslotDay)
                return recurringOrder;

            var proposedStartDate = recurringOrder.StartDate.Value.EquivalentWeekDay(timeslotDay).Date;
            var proposedEndDate = recurringOrder.EndDate.GetValueOrDefault().Value.EquivalentWeekDay(timeslotDay).Date;

            // If the proposed start date is not in the future then the new start
            // date needs to be in the next week.
            if (DateTime.Today >= proposedStartDate)
                proposedStartDate = proposedStartDate.AddWeeks(1);

            // If the start date is the next day, but we are past the cut off time then the new
            // start date needs to be in the next week.
            if (DateTime.Today.AddDays(1) == proposedStartDate && DateTime.Now.IsAfterCutOffTime())
                proposedStartDate = proposedStartDate.AddWeeks(1);

            recurringOrder.StartDate = proposedStartDate;
            recurringOrder.EndDate = recurringOrder.WeeklyRecurrence == WeeklyRecurrence.OneOff ? proposedStartDate : proposedEndDate;

            await AdjustSkipDates(recurringOrder);

            var updateDatesCmd = new UpdateCommand<RecurringOrder>(recurringOrder.Id, recurringOrder);
            recurringOrder = await _recurringOrdersHandler.ExecuteAsync(updateDatesCmd);

            return recurringOrder;
        }

        private async Task AdjustSkipDates(RecurringOrder recurringOrder)
        {
            foreach (var skipDate in recurringOrder.SkipDates.OrEmptyIfNull().Where(s => s.Date.Value >= DateTime.Today))
            {
                if (skipDate.Date.Value.DayOfWeek != recurringOrder.Timeslot.DayOfWeek)
                {
                    var newDate = skipDate.Date.Value.EquivalentWeekDay(recurringOrder.Timeslot.DayOfWeek);
                    await _skipDatesHandler.ExecuteAsync(new DeleteSkipDateCommand(recurringOrder.Id, skipDate.Date));
                    await _skipDatesHandler.ExecuteAsync(new CreateSkipDateCommand(recurringOrder.Id, newDate, skipDate.CreatedBy, skipDate.Reason));
                }
            }
        }

        //TODO - remove this method when no longer used by web
        public async Task<OrderDeliveryContext<RecurringOrder>> GetRecurringOrderForDateAsync(Guid recurringOrderId, Date deliveryDate)
        {
            var recurringOrder = await GetRecurringOrderAsync(recurringOrderId);

            if (recurringOrder == null)
                return null;

            var customer = await _customersHandler.ExecuteAsync(new GetCustomerByContactIdQuery(recurringOrder.ContactId));

            var orderForDate = new OrderDeliveryContext<RecurringOrder>
            {
                Order = recurringOrder,
                IsDeliveryChargeExempt = customer.IsDeliveryChargeExempt,
                Discounts = await _discountService.GetDiscountsForDeliveryDate(customer, deliveryDate)
            };

            if (!customer.IsDeliveryChargeExempt)
            {
                var chargeableorderId = await GetChargeableOrderId(recurringOrder.TimeslotId, deliveryDate, recurringOrder.ContactId);

                orderForDate.DeliveryCharge = recurringOrder.Timeslot.DeliveryCharge;
                orderForDate.DeliveryChargeableRecurringOrderId = chargeableorderId.GetValueOrDefault();
            }
            return orderForDate;
        }
        
        public async Task<DeliveryChargeResult> GetDeliveryChargeForTimeslotDateAsync(Guid customerId, Guid timeslotId, Guid? recurringOrderId, Date? deliveryDate)
        {
            var result = new DeliveryChargeResult();
            
            var customer = await _customersHandler.ExecuteAsync(new GetByIdQuery<Customer>(customerId));

            if (customer.IsDeliveryChargeExempt)
                return result;

            deliveryDate = deliveryDate ?? DateTime.Now.ToBritishTime();
            var timeslot = await _timeslotsService.GetTimeslot(timeslotId);
            if(timeslot.DeliveryCharge == 0)
                return result;

            DateTime timeslotDate = deliveryDate.Value.Value.NextWeekday(timeslot.DayOfWeek, true).GetNextAvailableDeliveryDate();
            var chargeableorderId = await GetChargeableOrderId(timeslotId, timeslotDate, customer.PrimaryContact.Id);

            if (chargeableorderId == null || chargeableorderId == recurringOrderId)
            {
                result.DeliveryCharge = timeslot.DeliveryCharge;
            }
            else
                result.IsAlreadyCharged = true;

            return result;
        }

        private async Task<Guid?> GetChargeableOrderId(Guid timeslotId, Date deliveryDate, Guid contactId)
        {
            var orders = await _forecastService.GetForecastedOrdersAsync(DateRange.SingleDay(deliveryDate), contactId, true, true);

            if (!orders.ContainsKey(deliveryDate))
                return null;

            var chargeableorder = orders[deliveryDate]?.Where(o => o.TimeslotId == timeslotId).OrEmptyIfNull()
                               .OrderByDescending(o => o.WeeklyRecurrence)
                               .ThenBy(o => o.CreatedDate)
                               .FirstOrDefault();
            return chargeableorder?.RecurringOrderId;
        }
    }
}
