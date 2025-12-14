using Fed.Core.Entities;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fed.Web.Portal.Models.Orders
{
    public class Order
    {
        [Required]
        public string OrderName { get; set; }

        public string ContactId { get; set; }

        [Required]
        public string DeliveryAddressId { get; set; }

        public string BillingAddressId { get; set; }

        [Required]
        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public string Reccurence { get; set; }

        [Required]
        public string TimeSlotId { get; set; }

        public List<OrderItem> OrderItems { get; set; }

        public IList<Product> Products { get; set; }

        public IList<Timeslot> TimeSlots { get; set; }

        public List<DeliveryAddress> DeliveryAddress { get; set; }

        public string CustomerId { get; set; }

        public string OrderId { get; set; }

        public bool FreeOrder { get; set; }

        public string TotalPrice { get; set; }

        public string CompanyName { get; set; }

        public string CustomerShortId { get; set; }

    }
}
