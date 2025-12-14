using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.AzureFunctions.Entities
{
    public class SupplierPurchaseOrderRequest
    {
        public int SupplierId { get; set; }
        public Guid? ProductCategoryId { get; set; }
        public List<string> EmailAddresses { get; set; }
        public string EmailSubject { get; set; }
    }
}
