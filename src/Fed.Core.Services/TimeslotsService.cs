using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Extensions;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class TimeslotsService : ITimeslotsService
    {
        private readonly ITimeslotsHandler _timeslotsHandler;
        private readonly IForecastService _forecastService;

        public TimeslotsService(ITimeslotsHandler handler,
                                IForecastService forecastService)
        {
            _timeslotsHandler = handler ?? throw new ArgumentNullException(nameof(handler));
            _forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
        }


        public async Task<IList<Timeslot>> GetTimeslots(Guid hubId, bool onlyAvailable, bool allHubs = false)
        {
            var query = new GetTimeslotsQuery(hubId, false, allHubs);
            var timeslots = await _timeslotsHandler.ExecuteAsync(query);

            if (timeslots == null || timeslots.Count == 0)
                return null;
            
            //ToDo - implement fast cached availability
            //await GetAvailability(timeslots);

            if (onlyAvailable)
                timeslots = timeslots.Where(t => t.AvailableCapacity > 0).ToList();

            return timeslots;

        }

        public async Task<Timeslot> GetTimeslot(Guid timeslotId)
        {
            var timeslot = await _timeslotsHandler.ExecuteAsync(new GetByIdQuery<Timeslot>(timeslotId));

            //ToDo - implement fast cached availability
            //await GetAvailability(timeslot);

            return timeslot;

        }

        private async Task GetAvailability(IList<Timeslot> timeslots)
        {
            var fromDate = DateTimeExtensions.GetNextAvailableWorkingDay();
            var toDate = fromDate.AddDays(6);

            var forecast = await _forecastService.GetForecastDeliveriesAsync(DateRange.Create(fromDate, toDate));
            foreach (var f in forecast.Values.SelectMany(d => d.ToList()))
            {
                var timeslot = timeslots.Single(t => t.Id == f.TimeslotId);
                timeslot.AvailableCapacity = timeslot.TotalCapacity - (f.DeliveryAddressIds?.Count() ?? 0);
            }
        }

        private async Task GetAvailability(Timeslot timeslot)
        {
            var nextTimeslotDate = DateTime.Now.NextWeekday(timeslot.DayOfWeek).GetNextAvailableDeliveryDate();
            int deliveries = 0;
            var forecast = await _forecastService.GetForecastDeliveriesAsync(DateRange.SingleDay(nextTimeslotDate));
            if(forecast.Count > 0 && forecast.ContainsKey(nextTimeslotDate))
                deliveries = forecast[nextTimeslotDate].Where(f => f.TimeslotId == timeslot.Id)?.FirstOrDefault()?.DeliveryAddressIds?.Count() ?? 0;
            timeslot.AvailableCapacity = timeslot.TotalCapacity - deliveries;
        }
    }
}
