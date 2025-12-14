using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fed.Core.Enums
{
    public enum DiscountQualificationType
    {
        [Display(Name = "Minimum value of order")]
        OrderValue = 1,
        [Display(Name = "Purchase within product categories")]
        CategorySpend = 2,
        [Display(Name = "Purchase of specific products")]
        ProductPurchase = 3
    }
}
