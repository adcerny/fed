using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Web.SupplierPortal.ViewModels
{
    public class DeliveryForecastViewModel
    {
        public DeliveryForecastViewModel(
            Date fromDate,
            Date toDate,
            IList<Timeslot> timeslots,
            IList<(TimeSpan, TimeSpan)> timeslotRows,
            IDictionary<DateTime, IList<ForecastedDeliveries>> forecast)
        {
            FromDate = fromDate;
            ToDate = toDate;
            Timeslots = timeslots;
            TimeslotRows = timeslotRows;
            Forecast = forecast;
        }

        public Date FromDate { get; set; }
        public Date ToDate { get; set; }
        public IList<Timeslot> Timeslots { get; set; }
        public IDictionary<DateTime, IList<ForecastedDeliveries>> Forecast { get; set; }
        public IList<(TimeSpan, TimeSpan)> TimeslotRows { get; set; }
    }
}