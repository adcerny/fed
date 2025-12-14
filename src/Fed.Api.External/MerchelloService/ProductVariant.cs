namespace Fed.Api.External.MerchelloService
{
    public class ProductVariant
    {
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public string SupplierSku { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }
        public string Sku { get; set; }
    }
}