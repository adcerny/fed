namespace Fed.Core.Models
{
    public class CardPaymentRequest
    {
        public string CardHolderName { get; set; }
        public string DeviceData { get; set; }
        public string PaymentMethodNonce { get; set; }
        public string AddressLine1 { get; set; }
        public string Postcode { get; set; }
        public bool IsPrimary { get; set; }
    }
}
