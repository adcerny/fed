using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fed.Core.Services.Common
{
    public class OrderValueQualificationStrategy : IDiscountQualificationStrategy
    {
        public DiscountQualification GetQualification(Discount discount, IList<LineItem> items)
        {
            var qualification = new DiscountQualification();

            var total = items.Sum(i => i.Price * i.Quantity);
            qualification.IsQualified = total >= discount.MinOrderValue.GetValueOrDefault();
            qualification.AmountNeededToQualify = Math.Max(discount.MinOrderValue.GetValueOrDefault() - total, 0);

            return qualification;
        }
    }
}
