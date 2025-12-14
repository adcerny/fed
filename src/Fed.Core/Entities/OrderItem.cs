using System;

namespace Fed.Core.Entities
{
    public class OrderItem
    {
        public Guid OrderId { get; set; }
        public string ProductId { get; set; }
        public int Quantity { get; set; }

        public string ProductCode { get; set; }
        public string ProductGroup { get; set; }
        public string ProductName { get; set; }
        public string SupplierId { get; set; }
        public string SupplierSKU { get; set; }
        public decimal Price { get; set; }
        public decimal RefundablePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public bool IsTaxable { get; set; }

        public decimal ActualPrice => SalePrice ?? Price;
    }
}