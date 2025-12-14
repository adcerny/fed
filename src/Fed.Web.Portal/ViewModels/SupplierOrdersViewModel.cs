using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System.Collections.Generic;

namespace Fed.Web.Portal.ViewModels
{
    public class SupplierOrdersViewModel
    {
        public SupplierOrdersViewModel(
            string supplier,
            int supplierId,
            Date deliveryDate,
            IList<SupplierProductQuantity> orders)
        {
            Supplier = supplier;
            SupplierId = supplierId;
            DeliveryDate = deliveryDate;
            Orders = orders;
        }

        public string Supplier { get; set; }

        public int SupplierId { get; set; }
        public Date DeliveryDate { get; set; }
        public IList<SupplierProductQuantity> Orders { get; set; }
    }
}