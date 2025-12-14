using Fed.Core.Enums;
using Fed.Core.ValueTypes;
using System;

namespace Fed.Core.Models
{
    public class ForecastedOrder
    {
        public Guid RecurringOrderId { get; set; }
        public Guid ContactId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid DeliveryAddressId { get; set; }
        public WeeklyRecurrence WeeklyRecurrence { get; set; }
        public Guid TimeslotId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public Date StartDate { get; set; }
        public Date? EndDate { get; set; }
        public Date? LastDeliveryDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool SplitDeliveriesByOrder { get; set; }
    }
}
