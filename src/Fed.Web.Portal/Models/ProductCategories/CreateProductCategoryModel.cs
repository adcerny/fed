using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Models.ProductCategories
{
    public class CreateProductCategoryModel
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(250)]
        public string Name { get; set; }

        public static CreateProductCategoryModel CreateEmpty() =>
           new CreateProductCategoryModel
           {
               Name = string.Empty
           };
    }
}
