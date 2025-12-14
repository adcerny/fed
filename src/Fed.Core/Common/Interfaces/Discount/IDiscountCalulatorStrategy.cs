using Fed.Core.Models;
using System.Collections.Generic;

namespace Fed.Core.Common.Interfaces
{
    public interface IDiscountCalculator
    {
        DiscountResult CalculateDiscount(IList<LineItem> items);

        IList<LineItem> GetEligableProducts(IList<LineItem> items);

        DiscountQualification GetQualification(IList<LineItem> items);
    }
}