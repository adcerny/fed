using Fed.Core.Entities;
using Fed.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Common.Interfaces
{
    public interface IDiscountQualificationStrategy
    {
        DiscountQualification GetQualification(Discount discount, IList<LineItem> items);
    }
}
