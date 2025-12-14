using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Extensions;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fed.Core.Services.Common
{
    public class ProductPurchaseQualificationStrategy : IDiscountQualificationStrategy
    {
        public DiscountQualification GetQualification(Discount discount, IList<LineItem> items)
        {
            var qualification = new DiscountQualification();

            qualification.IsQualified = IsQualified(discount, items);
            return qualification;
        }

        private bool IsQualified(Discount discount, IList<LineItem> items)
        {
            foreach(var qualifyingProduct in discount.QualifyingProducts.OrEmptyIfNull())
            {
                if (!items.Any(i => i.ProductCode == qualifyingProduct.ProductCode && i.Quantity >= qualifyingProduct.Quantity))
                    return false;
            }
            return true;
        }
    }
}
