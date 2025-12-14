using System;

namespace Fed.Core.Entities
{
    public class OrderDiscount
    {
        public Guid OrderId { get; set; }
        public Guid DiscountId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? Percentage { get; set; }
        public decimal? Value { get; set; }
        public decimal? OrderTotalDeduction { get; set; }
        public Guid DiscountedOrderId { get; set; }
    }
}
