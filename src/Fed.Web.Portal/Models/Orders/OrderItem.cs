namespace Fed.Web.Portal.Models.Orders
{
    public class OrderItem
    {
        public string ProductId { get; set; }
        public string ProductCode { get; set; }
        public string Quantity { get; set; }
        public string Price { get; set; }
        public double? SalePrice { get; set; }
    }
}
