using System.ComponentModel.DataAnnotations;

namespace Fed.Web.Portal.Models.Suppliers
{
    public class SupplierInfoModel
    {
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        public string SupplierName { get; set; }

        public SupplierInfoModel(int supplierId, string supplierName)
        {
            SupplierId = supplierId;
            SupplierName = supplierName;
        }
    }
}
