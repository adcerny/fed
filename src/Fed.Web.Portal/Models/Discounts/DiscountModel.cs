using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Models.Discounts
{
    public class DiscountModel
    {
        public DiscountModel()
        {
            Discount = new Discount();
            Discount.AppliedStartDate = DateTime.Today;
        }

        public Discount Discount { get; set; }
        public IList<Product> Products { get; set; }
        public IList<ProductCategory> ProductCategories { get; set; }
    }
}
