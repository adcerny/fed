using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace Fed.Core.Services.Common
{
    public class ValueDiscountStrategy : DiscountStrategy, IDiscountCalculator
    {

        public ValueDiscountStrategy(Discount discount,
                                          IDiscountQualificationStrategy qualificationStrategy,
                                          IDiscountEligibleProductsStrategy eligibleProductsStrategy) :
            base(discount, qualificationStrategy, eligibleProductsStrategy)
        {
        }


        public override DiscountResult CalculateDiscount(IList<LineItem> items)
        {
            var result = new DiscountResult { DiscountId = _discount.Id, DiscountName = _discount.Name };
            result.DiscountQualification = _qualificationStrategy.GetQualification(_discount, items);

            if (result.DiscountQualification.IsQualified)
                result.DiscountAmount = _discount.Value.GetValueOrDefault();

            return result;
        }
    }
}
