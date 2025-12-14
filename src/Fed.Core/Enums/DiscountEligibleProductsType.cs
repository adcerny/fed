using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fed.Core.Enums
{
    public enum DiscountEligibleProductsType
    {
        [Display(Name = "All products")]
        AllProducts = 1,
        [Display(Name = "Products within specific categories")]
        Category  = 2
    }
}
