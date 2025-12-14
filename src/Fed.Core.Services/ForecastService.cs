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
    public class ForecastService : IForecastService
    {
        private readonly IRecurringOrdersHandler _recurringOrdersHandler;
        private readonly IForecastedOrdersHandler _forecastedOrdersHandler;
        private readonly IOrdersHandler _ordersHandler;
        private readonly IHolidaysHandler _holidaysHandler;
        private readonly ISkipDatesHandler _skipDatesHandler;

        public ForecastService(
            IRecurringOrdersHandler recurringOrdersHandler,
            IForecastedOrdersHandler forecastedOrdersHandler,
            IOrdersHandler ordersHandler,
            IHolidaysHandler holidaysHandler,
            ISkipDatesHandler skipDatesHandler)
        {
            _recurringOrdersHandler =
                recurringOrdersHandler
                ?? throw new ArgumentNullException(nameof(recurringOrdersHandler));

            _forecastedOrdersHandler =
                forecastedOrdersHandler
                ?? throw new ArgumentNullException(nameof(forecastedOrdersHandler));

            _ordersHandler =
                ordersHandler
                ?? throw new ArgumentNullException(nameof(ordersHandler));

            _holidaysHandler =
                holidaysHandler
                ?? throw new ArgumentNullException(nameof(holidaysHandler));

            _skipDatesHandler =
                skipDatesHandler
                ?? throw new ArgumentNullException(nameof(skipDatesHandler));
        }

        private bool IsHolidayOrSkipDate(IList<Date> holidays, IList<SkipDate> skipDates, Date date) =>
            (holidays != null && holidays.Contains(date))
            || (skipDates != null && skipDates.OrEmptyIfNull().Select(d => d.Date).Contains(date));



        public async Task<IDictionary<Date, IList<RecurringOrder>>> GetForecastAsync(
            DateRange forecastPeriod,
            Guid? contactId = null,
            bool includeDemo = false,
            bool includePaused = false)
        {

            var recurringOrderForecast = new Dictionary<Date, IList<RecurringOrder>>();

            var forecast = await GetForecastedOrdersAsync(forecastPeriod, contactId, includeDemo, includePaused);

            var ids = forecast.SelectMany(d => d.Value)
                                            .Select(o => o.RecurringOrderId.ToString())
                                            .Distinct()
                                            .ToList();

            var recurringOrders = await _recurringOrdersHandler.ExecuteAsync(new GetByIdsQuery<RecurringOrder>(ids));

            foreach (var date in forecast.Keys)
            {

                var orders = forecast[date].Select(fo => recurringOrders.Where(ro => ro.Id == fo.RecurringOrderId).Single());
                recurringOrderForecast.Add(date, orders.ToList());

            }
            return recurringOrderForecast;
        }

        public async Task<IDictionary<Date, IList<Guid>>> GetForecastRecurringOrderIdsAsync(
            DateRange forecastPeriod,
            Guid? contactId = null,
            bool includeDemo = false,
            bool includePaused = false)
        {
            var idsForecast = new Dictionary<Date, IList<Guid>>();
            var forecast = await GetForecastAsync(forecastPeriod, contactId, includeDemo, includePaused);
            foreach (var date in forecast.Keys)
            {
                idsForecast.Add(date, forecast[date].Select(o => o.Id).ToList());
            }
            return idsForecast;
        }

        public async Task<IDictionary<Date, IList<ForecastedDeliveries>>> GetForecastDeliveriesAsync(
            DateRange forecastPeriod,
            bool includeDemo = false,
            bool includePaused = false)
        {
            var deliveryForecast = new Dictionary<Date, IList<ForecastedDeliveries>>();
            var forecast = await GetForecastedOrdersAsync(forecastPeriod, null, includeDemo, includePaused);
            foreach (var date in forecast.Keys)
            {
                var orders = forecast[date];
                var ordersGroupedByTimeslot = orders.GroupBy(o => o.TimeslotId).ToList();
                var timeslotDeliveries = new List<ForecastedDeliveries>();

                foreach (var ordersByTimeslot in ordersGroupedByTimeslot)
                {
                    
                    var timeslotId = ordersByTimeslot.First().TimeslotId;

                    var deliveries = new ForecastedDeliveries { TimeslotId = timeslotId };

                    var timeslotBatch = ordersByTimeslot.ToList();
                    var ordersGroupedByContact =
                    timeslotBatch
                        .GroupBy(o => new { o.ContactId, DeliveryGroup = (o.SplitDeliveriesByOrder ? o.RecurringOrderId : o.DeliveryAddressId) });

                    foreach (var ordersByContact in ordersGroupedByContact)
                    {
                        var deliveryBatch = ordersByContact.First();

                        deliveries.DeliveryAddressIds.Add(deliveryBatch.DeliveryAddressId);
                    }
                    timeslotDeliveries.Add(deliveries);
                }
                deliveryForecast.Add(date, timeslotDeliveries);
            }
            return deliveryForecast;
        }

        public async Task<IDictionary<Date, IList<Guid>>> GetForecastCustomerIdsAsync(
            DateRange forecastPeriod,
            bool includeDemo = false,
            bool includePaused = false)
        {
            var idsForecast = new Dictionary<Date, IList<Guid>>();
            var forecast = await GetForecastedOrdersAsync(forecastPeriod, null, includeDemo, includePaused);
            foreach (var date in forecast.Keys)
            {
                idsForecast.Add(date, forecast[date].Select(o => o.CustomerId)?.Distinct()?.ToList());
            }
            return idsForecast;
        }

        public async Task<IDictionary<Date, IList<ForecastedOrder>>> GetForecastedOrdersAsync(
           DateRange forecastPeriod,
           Guid? contactId = null,
           bool includeDemo = false,
           bool includePaused = false)
        {
            // Create an empty forecast first.
            // The forecast will be a dictionary of dates and a list of recurring orders
            // for each date, sorted by date.
            var forecast = new SortedDictionary<Date, IList<ForecastedOrder>>();

            // Load all recurring orders next
            var getRecurringOrdersQuery =
                new GetRecurringOrdersQuery(
                    forecastPeriod,
                    contactId,
                    includeExpired: false,
                    includeFromDeletedAccounts: false,    // Deleted account orders should not get processed
                    includeFromDemoAccounts: includeDemo, // Demo account orders are optionally processed
                    includeFromPausedAccounts: includePaused, // Paused account orders are optionally processed
                    includeFromCancelledAccounts: false); // Cancelled account orders should not get processed

            var orders = await _forecastedOrdersHandler.ExecuteAsync(getRecurringOrdersQuery);

            if (orders == null)
                return forecast;

            // Load all bank holidays
            var getHolidaysQuery = new GetHolidaysQuery(forecastPeriod);
            var holidays = await _holidaysHandler.ExecuteAsync(getHolidaysQuery);
            var skipDates = await _skipDatesHandler.ExecuteAsync(new GetSkipDatesQuery(null, forecastPeriod));

            // Next iterate through all existing recurring orders and establish if a given
            // recurring order will produce any orders which fall into the specified time range:
            foreach (var recurringOrder in orders)
            {
                //establish what day of the week the order is for
                var deliveryDay = recurringOrder.DayOfWeek;

                // Based on the timeslot's day of week and the recurring order's start date
                // we need to establish if the first order will be in the same week or the following week:
                var delta =
                    deliveryDay
                    - recurringOrder.StartDate.Value.DayOfWeek;

                var daysUntilFirstOrder = delta >= 0 ? delta : 7 + delta;
                var firstOrderDate = recurringOrder.StartDate.Value.AddDays(daysUntilFirstOrder);

                // Now that we have the very first order date of this particular recurring order,
                // we need to establish the first order date which falls into the forecast period:
                var orderDate = firstOrderDate;

                if (recurringOrder.WeeklyRecurrence != WeeklyRecurrence.OneOff)
                    while (orderDate < forecastPeriod.From.Value)
                        orderDate = orderDate.AddWeeks(recurringOrder.WeeklyRecurrence);

                // If a recurring order is a one off and the start date is
                // past the from forecast date then skip this order.
                else if (recurringOrder.WeeklyRecurrence == WeeklyRecurrence.OneOff
                    && recurringOrder.StartDate.Value < forecastPeriod.From.Value)
                    continue;

                //// Now that we have the theoretical first order date of this recurring order
                //// (which also falls into the forecast period) we need to double check if there wasn't
                //// already a fulfilled order booked for it. If there is already an order booked for that
                //// day then presumably we have already delivered the products and this order date should 
                //// get excluded from the forecast:
                if (recurringOrder.LastDeliveryDate.HasValue && recurringOrder.LastDeliveryDate >= recurringOrder.StartDate)
                {
                    if (recurringOrder.WeeklyRecurrence == WeeklyRecurrence.OneOff)
                        // If the recurring order was a one off order and it has already been
                        // processed then skip and proceed to the next recurring order
                        continue;

                    var nextOrderDate = recurringOrder.LastDeliveryDate.Value.AddWeeks(recurringOrder.WeeklyRecurrence).Value.EquivalentWeekDay(deliveryDay);

                    if (nextOrderDate >= forecastPeriod.From.Value)
                        orderDate = nextOrderDate;
                }

                // In the next section we need to calculate each order date - starting
                // from the previously established first order date - until we either
                // reach the end of the forecast period or the end of the recurring
                // order time frame. Pick the smaller date of the two:
                var endDate =
                    recurringOrder.EndDate <= forecastPeriod.To
                    ? recurringOrder.EndDate
                    : forecastPeriod.To;

                // Sometimes a customer chooses to skip an order. Load all skip dates
                // for the current recurring order so we can take these into account:

                // While the order date still falls into the forecast period keep
                // adding it into the forecast dictionary if it isn't a holiday,
                // skip date or a one off order:
                while (orderDate <= endDate)
                {
                    // Check if the order date falls on a bank holiday or skip date
                    if (!IsHolidayOrSkipDate(holidays.Select(h => h.Date).ToList(),
                                             skipDates?.Where(s => s.RecurringOrderId == recurringOrder.RecurringOrderId).ToList(),
                                             orderDate))
                    {
                        if (!forecast.ContainsKey(orderDate))
                            forecast.Add(orderDate, new List<ForecastedOrder>());

                        forecast[orderDate].Add(recurringOrder);
                    }

                    if (recurringOrder.WeeklyRecurrence == WeeklyRecurrence.OneOff)
                        break;

                    orderDate = orderDate.AddWeeks(recurringOrder.WeeklyRecurrence);
                }
            }

            // Return the forecast
            return forecast;
        }
    }
}