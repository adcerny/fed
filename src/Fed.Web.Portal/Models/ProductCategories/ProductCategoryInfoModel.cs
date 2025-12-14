using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Models.ProductCategories
{
    public class ProductCategoryInfoModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(250)]
        public string Name { get; set; }

        public ProductCategoryInfoModel(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
