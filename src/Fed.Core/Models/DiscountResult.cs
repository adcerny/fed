using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Models
{
    public class DiscountResult
    {
        public DiscountQualification DiscountQualification { get; set; }

        public decimal DiscountAmount { get; set; }
        
        public Guid DiscountId { get; set; }

        public string DiscountName { get; set; }

        public string DiscountMessage { get; set; }

        public IList<LineItem> DiscountedProducts { get; set; }

        public IList<LineItem> EligibleProducts { get; set; }
    }
}
