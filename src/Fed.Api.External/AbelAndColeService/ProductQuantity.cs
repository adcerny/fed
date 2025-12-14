namespace Fed.Api.External.AbelAndColeService
{
    public class ProductQuantity
    {
        public static ProductQuantity New(int productId, int quantiyt) =>
            new ProductQuantity { ProductId = productId, Quantity = quantiyt };

        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
