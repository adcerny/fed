using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fed.Core.Services.Common
{
    public class PercentageDiscountStrategy : DiscountStrategy
    {
        public PercentageDiscountStrategy(Discount discount,
                                          IDiscountQualificationStrategy qualificationStrategy,
                                          IDiscountEligibleProductsStrategy eligibleProductsStrategy) :
            base(discount, qualificationStrategy, eligibleProductsStrategy)
        {
        }

        public override DiscountResult CalculateDiscount(IList<LineItem> items)
        {
            var result = new DiscountResult { DiscountId = _discount.Id, DiscountName = _discount.Name };

            result.EligibleProducts = GetEligableProducts(items);
            result.DiscountQualification = GetQualification(items);

            if (result.DiscountQualification.IsQualified)
            {
                var total = result.EligibleProducts.Sum(i => i.Price * i.Quantity);
                var eligableTotal = Math.Min(total, _discount.MaxOrderValue ?? total);
                result.DiscountAmount = _discount.GetPercentageDiscount(eligableTotal);
            }

            return result;
        }
    }
}
