using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Services.Common
{
    public class ProductDiscountStrategy : DiscountStrategy, IDiscountCalculator
    {

        public ProductDiscountStrategy(Discount discount, 
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
                result.DiscountedProducts = _discount.DiscountedProducts;
            }

            return result;
        }
    }
}
