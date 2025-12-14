using System.ComponentModel.DataAnnotations;

namespace Fed.Web.Portal.Models.Suppliers
{
    public class CreateSupplierModel
    {
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        public string SupplierName { get; set; }

        public static CreateSupplierModel CreateEmpty() =>
           new CreateSupplierModel
           {
               SupplierName = string.Empty
           };
    }
}
