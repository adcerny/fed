using Fed.Core.ValueTypes;
using Fed.Web.Service.Client;
using Fed.Web.SupplierPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Fed.Core.Extensions;
using System.Collections.Generic;
using Fed.Core.Entities;

namespace Fed.Web.SupplierPortal.Controllers
{
    public class DeliveryForecastController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;

        public DeliveryForecastController(IConfiguration config, FedWebClient fedWebClient)
        {
            _config = config;
            _fedWebClient = fedWebClient;
        }

        [HttpGet("/deliveryForecast")]
        public async Task<IActionResult> GetDeliveryForecastAsync([FromRoute]int supplierId, [FromRoute]string supplierName)
        {
            var fromDate = DateTimeExtensions.GetNextAvailableWorkingDay();
            var toDate = fromDate.AddDays(6);


            var forecast = await _fedWebClient.GetForecastDeliveriesAsync(fromDate, toDate);

            var hubs = await _fedWebClient.GetHubsAsync();
            var timeslots = await _fedWebClient.GetTimeslotsAsync(hubs.First().Id);

            var activeTimeslots = timeslots.Where(t => forecast.Values.SelectMany(a => a.Select(b => b.TimeslotId)).Any()).OrderBy( t => t.EarliestTime).ToList();

            var timeslotRows = activeTimeslots.GroupBy(t => (t.EarliestTime, t.LatestTime)).Select(g => g.Key).OrderBy(t => t.EarliestTime).ToList();

            var viewModel = new DeliveryForecastViewModel(fromDate, toDate, activeTimeslots, timeslotRows, forecast);

            return View("DeliveryForecast", viewModel);
        }
    }
}