using System;

namespace Fed.Web.Portal.Models.Deliveries
{
    public class DeliveryAdditionModel
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
        public Guid DeliveryShortageId { get; set; }
    }
}