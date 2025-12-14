using Fed.Core.Services.Common;
using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using Fed.Core.Data.Handlers;

namespace Fed.Core.Services.Factories
{
    public class DiscountStrategyFactory : IDiscountStrategyFactory
    {
        public IDiscountCalculator GetCalculator(Discount discount)
        {
            switch (discount.RewardType)
            {
                case DiscountRewardType.Percentage:
                    return new PercentageDiscountStrategy(discount, GetQualificationStrategy(discount), GetEligibleProductsStrategy(discount)) as IDiscountCalculator;
                case DiscountRewardType.Value:
                    return new ValueDiscountStrategy(discount, GetQualificationStrategy(discount), GetEligibleProductsStrategy(discount)) as IDiscountCalculator;
                case DiscountRewardType.Product:
                    return new ProductDiscountStrategy(discount, GetQualificationStrategy(discount), GetEligibleProductsStrategy(discount)) as IDiscountCalculator;
                default:
                    throw new NotImplementedException();
            }
        }
        private IDiscountQualificationStrategy GetQualificationStrategy(Discount discount)
        {
            switch (discount.QualificationType)
            {
                case DiscountQualificationType.OrderValue:
                    return new OrderValueQualificationStrategy() as IDiscountQualificationStrategy;
                case DiscountQualificationType.CategorySpend:
                    return new CategorySpendEligibleStrategy() as IDiscountQualificationStrategy;
                case DiscountQualificationType.ProductPurchase:
                    return new ProductPurchaseQualificationStrategy() as IDiscountQualificationStrategy;
                default:
                    throw new NotImplementedException();
            }
        }

        private IDiscountEligibleProductsStrategy GetEligibleProductsStrategy(Discount discount)
        {
            switch (discount.EligibleProductsType)
            {
                case DiscountEligibleProductsType.AllProducts:
                    return new EligibleProductsAllStrategy() as IDiscountEligibleProductsStrategy;
                case DiscountEligibleProductsType.Category:
                    return new EligibleProductsByCategoryStrategy() as IDiscountEligibleProductsStrategy;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
