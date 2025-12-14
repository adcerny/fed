using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IForecastService
    {
        Task<IDictionary<Date, IList<RecurringOrder>>> GetForecastAsync(DateRange forcastPeriod, Guid? contactId = null, bool includeDemoAccounts = false, bool includePausedAccounts = false);

        Task<IDictionary<Date, IList<Guid>>> GetForecastRecurringOrderIdsAsync(DateRange forcastPeriod, Guid? contactId = null, bool includeDemoAccounts = false, bool includePausedAccounts = false);

        Task<IDictionary<Date, IList<Guid>>> GetForecastCustomerIdsAsync(DateRange forcastPeriod, bool includeDemoAccounts = false, bool includePausedAccounts = false);

        Task<IDictionary<Date, IList<ForecastedDeliveries>>> GetForecastDeliveriesAsync(DateRange forecastPeriod, bool includeDemo = false, bool includePausedAccounts = false);

        Task<IDictionary<Date, IList<ForecastedOrder>>> GetForecastedOrdersAsync(DateRange forcastPeriod, Guid? contactId = null, bool includeDemoAccounts = false, bool includePausedAccounts = false);
    }
}