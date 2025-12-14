using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class ForecastController : ControllerBase
    {
        private readonly IForecastService _forecastService;

        public ForecastController(IForecastService forecastService)
        {
            _forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
        }

        /// <summary>
        /// Returns a dictionary specifying a list of recurring orders for each day within a given date range.
        /// </summary>
        /// <param name="fromDate">The first day of the forecast period.</param>
        /// <param name="toDate">The last day of the forecast period.</param>
        /// <param name="contactId">Optional parameter to filter by contact.</param>
        [HttpGet("/forecast")]
        public async Task<ActionResult<IDictionary<Date, IList<RecurringOrder>>>> Forecast(
            [FromQuery]DateTime fromDate,
            [FromQuery]DateTime toDate,
            [FromQuery]Guid? contactId = null)
        {
            var forecastPeriod = DateRange.Create(Date.Create(fromDate), Date.Create(toDate));
            var forecast = await _forecastService.GetForecastAsync(forecastPeriod, contactId);

            return Ok(forecast);
        }

        [HttpGet("/forecast/recurringOrderIds")]
        public async Task<ActionResult<IDictionary<Date, IList<Guid>>>> GetForecastRecurringOrderIds(
            [FromQuery]DateTime fromDate,
            [FromQuery]DateTime toDate,
            [FromQuery]Guid? contactId = null)
        {
            var forecastPeriod = DateRange.Create(Date.Create(fromDate), Date.Create(toDate));
            var forecast = await _forecastService.GetForecastRecurringOrderIdsAsync(forecastPeriod, contactId);

            return Ok(forecast);
        }

        [HttpGet("/forecast/customerIds")]
        public async Task<ActionResult<IDictionary<Date, IList<Guid>>>> GetForecastCustomerIds(
            [FromQuery]DateTime fromDate,
            [FromQuery]DateTime toDate)
        {
            var forecastPeriod = DateRange.Create(Date.Create(fromDate), Date.Create(toDate));
            var forecast = await _forecastService.GetForecastCustomerIdsAsync(forecastPeriod);

            return Ok(forecast);
        }

        [HttpGet("/forecast/deliveries")]
        public async Task<ActionResult<IDictionary<Date, IList<ForecastedDeliveries>>>> GetForecastDeliveries(
            [FromQuery]DateTime fromDate,
            [FromQuery]DateTime toDate)
        {
            var forecastPeriod = DateRange.Create(Date.Create(fromDate), Date.Create(toDate));
            var forecast = await _forecastService.GetForecastDeliveriesAsync(forecastPeriod);

            return Ok(forecast);
        }
    }
}