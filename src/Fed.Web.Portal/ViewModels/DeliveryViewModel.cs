using Fed.Core.Entities;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class DeliveryViewModel
    {
        public DeliveryViewModel(
            Delivery delivery,
            IList<Product> products,
            string nextDeliveryId,
            string prevDeliveryId,
            string fedLabelHostUrl)
        {
            Delivery = delivery;
            Products = products;
            NextDeliveryId = nextDeliveryId;
            PrevDeliveryId = prevDeliveryId;
            FedLabelHostUrl = fedLabelHostUrl;
        }

        public Delivery Delivery { get; }
        public IList<Product> Products { get; }

        public string NextDeliveryId { get; }
        public string PrevDeliveryId { get; }
        public string FedLabelHostUrl { get; }
    }
}