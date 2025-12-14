namespace Fed.Core.Models
{
    public class SupplierProductQuantity
    {
        public string SupplierId { get; set; }
        public string SupplierSKU { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int FedQuantity { get; set; }
        public int SupplierQuantity { get; set; }
        public int CustomerCount { get; set; }
        public string Customers { get; set; }
    }
}