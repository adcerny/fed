namespace Fed.Web.Portal.Models.Deliveries
{
    public class DeliveryShortageModel
    {
        public string ProductId { get; set; }
        public int DesiredQuantity { get; set; }
        public int ActualQuantity { get; set; }
        public decimal ProductPrice { get; set; }
        public string Reason { get; set; }
        public string ReasonCode { get; set; }
    }
}