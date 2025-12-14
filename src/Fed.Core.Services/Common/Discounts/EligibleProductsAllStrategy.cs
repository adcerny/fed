using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fed.Core.Services.Common
{
    public class EligibleProductsAllStrategy : IDiscountEligibleProductsStrategy
    {
        public IList<LineItem> GetProducts(Discount discount, IList<LineItem> items)
        {
            return items;
        }
    }
}
