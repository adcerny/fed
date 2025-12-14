using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fed.Core.Services.Common
{
    public class ProductSelectionByCategory : IDiscountEligibleProductsStrategy
    {
        public IList<LineItem> GetProducts(Discount discount, IList<LineItem> items)
        {
            if (discount.EligibleProductCategorySkus == null)
                return new List<LineItem>();

            return items.Where(i => discount.EligibleProductCategorySkus.Any(c => i.ProductCode.StartsWith(c))).ToList();
        }
    }
}
