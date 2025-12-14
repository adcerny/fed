using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Entities
{
    public class DiscountQualifyingCategory
    {
        public Guid ProductCategoryId { get; set; }
        public int ProductQuantity { get; set; }
        public IList<string> CategoryProductSkus { get; set; }
        
    }
}
