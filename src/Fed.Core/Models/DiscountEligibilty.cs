using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Models
{
    public class DiscountQualification
    {
        public bool IsQualified { get; set; }

        public decimal AmountNeededToQualify { get; set; }

        public string Message { get; set; }
    }
}
