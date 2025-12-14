using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Models
{
    public class DiscountCalculationQuery
    {
        public Guid? CustomerId { get; set; }
        public List<Guid> AppliedDiscountIds { get; set; }
        public Date? DeliveryDate { get; set; }
        public List<LineItem> OrderItems { get; set; }
    }
}
