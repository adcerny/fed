using System;

namespace Fed.Core.Entities
{
    public class RecurringOrderItem
    {
        public string ProductId { get; set; }
        public string ProductCode { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime AddedDate { get; set; }

        public decimal ActualPrice => SalePrice ?? Price ?? 0;
    }
}