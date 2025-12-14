using Fed.Core.ValueTypes;
using System;

namespace Fed.Core.Entities
{
    public class CustomerDiscount
    {
        public Guid CustomerId { get; set; }
        public Guid DiscountId { get; set; }
        public Discount Discount { get; set; }
        public DateTime? AppliedDate { get; set; }
        public Date? EndDate { get; set; }
        public string DiscountCode { get; set; }
        

    }
}
