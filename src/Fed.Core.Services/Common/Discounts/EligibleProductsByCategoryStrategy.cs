using Fed.Core.Common.Interfaces;
using Fed.Core.Data.Handlers;
using Fed.Core.Entities;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fed.Core.Services.Common
{
    public class EligibleProductsByCategoryStrategy : IDiscountEligibleProductsStrategy
    {
        public IList<LineItem> GetProducts(Discount discount, IList<LineItem> items)
        {
            if ((discount.EligibleProductCategorySkus?.Count ?? 0) == 0)
                return new List<LineItem>();
            
            return items.Where(i => discount.EligibleProductCategorySkus.Any(c => i.ProductCode == c))?.ToList();
        }
    }
}
