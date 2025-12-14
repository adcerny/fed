using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fed.Core.Entities
{
    public class RecurringOrder
    {
        public RecurringOrder(
            Guid id,
            string name,
            Guid contactId,
            Guid deliveryAddressId,
            Guid billingAddressId,
            DateTime startDate,
            DateTime endDate,
            WeeklyRecurrence weeklyRecurrence,
            Guid timeslotId,
            DateTime? createdDate = null,
            DateTime? lastUpdatedDate = null,
            bool isDeleted = false,
            DateTime? deletedDate = null,
            Timeslot timeslot = null,
            DeliveryAddress deliveryAddress = null,
            IList<RecurringOrderItem> orderItems = null,
            IList<SkipDate> skipDates = null,
            IList<Holiday> futureHolidays = null,
            bool isFree = false)
        {
            Id = id;
            Name = name;
            ContactId = contactId;
            DeliveryAddressId = deliveryAddressId;
            BillingAddressId = billingAddressId;
            StartDate = startDate;
            EndDate = endDate;
            WeeklyRecurrence = weeklyRecurrence;
            TimeslotId = timeslotId;
            OrderItems = orderItems ?? new List<RecurringOrderItem>();
            CreatedDate = createdDate ?? DateTime.UtcNow;
            LastUpdatedDate = lastUpdatedDate ?? CreatedDate;
            IsDeleted = IsDeleted;
            DeletedDate = deletedDate;
            Timeslot = timeslot;
            DeliveryAddress = deliveryAddress;
            SkipDates = skipDates;
            IsFree = isFree;
            NextDeliveryDate = GetNextDeliveryDate(futureHolidays);
        }

        public Guid Id { get; }
        public string Name { get; set; }
        public Guid ContactId { get; set; }
        public Guid DeliveryAddressId { get; set; }
        public Guid BillingAddressId { get; set; }
        public Date StartDate { get; set; }
        public Date? EndDate { get; set; }
        public WeeklyRecurrence WeeklyRecurrence { get; set; }
        public Guid TimeslotId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; }

        public DeliveryAddress DeliveryAddress { get; set; }

        private IList<RecurringOrderItem> _orderItems;
        public IList<RecurringOrderItem> OrderItems
        {
            get 
            {
                _orderItems?.RemoveAll(o => o.Quantity < 1);
                return _orderItems;
            }
            set 
            {
                value?.RemoveAll(o => o.Quantity < 1);
                _orderItems = value;
            }
        }

        public IList<SkipDate> SkipDates { get; }
        public bool IsFree { get; set; }
        public Date NextDeliveryDate { get; }

        public decimal TotalItemPrice => OrderItems.Sum(o => o.ActualPrice * o.Quantity);

        public Timeslot Timeslot { get; }

        public override string ToString()
        {
            var end = EndDate == Date.MaxDate ? "and onwards" : $"- EndDate";
            return $"{Name}: {StartDate} {end}, every {(int)WeeklyRecurrence}. {Timeslot.DayOfWeek}, {OrderItems.Count} Items";
        }

        public bool HasOrderItems() =>
            OrderItems != null
            && OrderItems.Count > 0;

        private IList<RecurringOrderItem> GetPositiveQuantityItems(IList<RecurringOrderItem> items) => 
            items.Where(i => i.Quantity > 0).ToList();

        private Date GetNextDeliveryDate(IList<Holiday> futureHolidays)
        {
            if (StartDate.Value != null)
            {
                var nextDelDate = Date.Create(StartDate.Value.NextWeekday(Timeslot?.DayOfWeek ?? StartDate.Value.DayOfWeek, false));
                if (EndDate < nextDelDate)
                    return Date.MinDate;

                if (WeeklyRecurrence == 0 && (SkipDates is object && SkipDates.Any(sd => sd.Date == nextDelDate) || futureHolidays is object && futureHolidays.Any(h => h.Date == nextDelDate)))
                    nextDelDate = Date.MinDate;

                if (WeeklyRecurrence > 0 && SkipDates is object && SkipDates.Count > 0)
                    while (SkipDates.Any(sd => sd.Date == nextDelDate))
                        nextDelDate = nextDelDate.AddWeeks(WeeklyRecurrence);

                if (WeeklyRecurrence > 0 && futureHolidays is object && futureHolidays.Count > 0)
                    while (futureHolidays.Any(h => h.Date == nextDelDate))
                        nextDelDate = nextDelDate.AddWeeks(WeeklyRecurrence);

                return nextDelDate;
            }
            else
            {
                return Date.MinDate;
            }
        }
    }
}
