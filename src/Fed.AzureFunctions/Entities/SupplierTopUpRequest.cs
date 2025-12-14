using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.AzureFunctions.Entities
{
    public class SupplierTopUpRequest
    {
        public int SupplierId { get; set; }
        public Guid? ProductCategoryId { get; set; }
        public List<int> MinQuantities { get; set; }
    }
}
