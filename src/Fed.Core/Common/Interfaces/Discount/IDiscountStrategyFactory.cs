using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Common.Interfaces
{
    public interface IDiscountStrategyFactory
    {
        IDiscountCalculator GetCalculator(Discount discount);
    }
}