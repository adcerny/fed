using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fed.Core.Services.Common
{
    public class CategorySpendEligibleStrategy : IDiscountQualificationStrategy
    {
        public DiscountQualification GetQualification(Discount discount, IList<LineItem> items)
        {
            
            var qualification = new DiscountQualification();
            if ((discount.QualifyingProductCategories?.Count ?? 0) > 0)
            {
                var qualifiedItems = items.Where(i => discount.QualifyingProductCategories.SelectMany(q => q.CategoryProductSkus).Any(c => i.ProductCode == c)).ToList();
                var total = qualifiedItems.Sum(i => i.Price * i.Quantity);

                qualification.IsQualified = total >= (discount.MinOrderValue.GetValueOrDefault()) && IsMinQuanityReached(discount, items);
                qualification.AmountNeededToQualify = Math.Max(discount.MinOrderValue.GetValueOrDefault() - total, 0);
            }

            return qualification;
        }
        private bool IsMinQuanityReached(Discount discount, IList<LineItem> items)
        {
            foreach(var category in discount.QualifyingProductCategories.Where(c => c.ProductQuantity >= 0))
            {
                var categoryItems = items.Where(i => category.CategoryProductSkus.Any(c => i.ProductCode == c)).ToList();
                if (categoryItems.Sum(c => c.Quantity) < category.ProductQuantity)
                    return false;
            }
            return true;
        }
    }
}
