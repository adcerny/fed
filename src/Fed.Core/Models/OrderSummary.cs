using Fed.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fed.Core.Models
{
    public class OrderSummary
    {
        public OrderSummary(
            Guid id,
            DateTime deliveryDate,
            string orderName,
            Guid timeslotId,
            TimeSpan earliestTime,
            TimeSpan latestTime,
            decimal totalPrice,
            WeeklyRecurrence weeklyRecurrence,
            DateTime dateAdded,
            IList<CategoryIcon> categoryIcons,
            string deliveryAddressDescription
            )
        {
            Id = id;
            DeliveryDate = deliveryDate;
            OrderName = orderName;
            TimeslotId = timeslotId;
            EarliestTime = earliestTime;
            LatestTime = latestTime;
            TotalPrice = totalPrice;
            WeeklyRecurrence = weeklyRecurrence;
            DateAdded = dateAdded;

            var orderedCategoryIcons =
                categoryIcons
                    .OrderByDescending(x => x.TotalQuantity)
                    .ThenByDescending(x => x.LineItemCount)
                    .Select(x => x.Icon)
                    .ToList();

            CategoryIcons = orderedCategoryIcons;
            DeliveryAddressDescription = deliveryAddressDescription;
        }

        public Guid Id { get; }
        public DateTime DeliveryDate { get; }
        public string OrderName { get; }
        public Guid TimeslotId { get; }
        public TimeSpan EarliestTime { get; }
        public TimeSpan LatestTime { get; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public WeeklyRecurrence WeeklyRecurrence { get; set; }
        public DateTime DateAdded { get; set; }
        public IList<string> CategoryIcons { get; }
        public string DeliveryAddressDescription { get; }
    }
}