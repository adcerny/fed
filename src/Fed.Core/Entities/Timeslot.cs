using System;

namespace Fed.Core.Entities
{
    public class Timeslot
    {
        public Guid Id { get; set; }
        public Guid HubId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan EarliestTime { get; set; }
        public TimeSpan LatestTime { get; set; }
        public int TotalCapacity { get; set; }
        public int AvailableCapacity { get; set; }
        public decimal DeliveryCharge { get; set; }
        public bool IsActive { get; set; }

        public override string ToString()
        {
            return $"{DayOfWeek} {EarliestTime.ToString(@"hh\:mm")} - {LatestTime.ToString(@"hh\:mm")}";
        }
    }
}