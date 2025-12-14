using System;

namespace Fed.Web.Portal.ViewModels
{
    public class SkipDeliveryDatesViewModel
    {
        public Guid OrderId { get; set; }

        public string CustomerId { get; set; }

        public DateTime SkipDate { get; set; }

        public string Reason { get; set; }

        public string CreatedBy { get; set; }

        public string RedirectTo { get; set; }
    }
}
