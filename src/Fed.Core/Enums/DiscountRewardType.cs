using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Fed.Core.Enums
{
    public enum DiscountRewardType
    {
        [Display(Name = "Percentage off total")]
        Percentage = 1,
        [Display(Name = "Fixed amount off total")]
        Value = 2,
        [Display(Name = "Discounted products")]
        Product = 3
    }
}
