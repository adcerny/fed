using Fed.Core.Entities;
using System;
using System.Collections.Generic;

namespace Fed.Core.Models
{
    public class OrderDeliveryContext<T>
    {
        public OrderDeliveryContext()
        {
            Discounts = new List<Discount>();
        }

        public T Order { get; set; }
        public decimal DeliveryCharge { get; set; }
        public bool IsDeliveryChargeExempt { get; set; }
        public Guid DeliveryChargeableRecurringOrderId { get; set; }
        public IList<Discount> Discounts { get; set; }
    }
}