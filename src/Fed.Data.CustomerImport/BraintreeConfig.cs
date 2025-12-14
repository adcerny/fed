namespace Fed.Data.CustomerImport
{
    public class BraintreeConfig
    {
        public string Environment { get; set; }
        public string MerchantId { get; set; }
        public string MerchantAccountId { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
    }
}
