using System;
using System.Collections.Generic;
using System.Text;

namespace Fed.Core.Models
{
    public class DeliveryChargeResult
    {
        public decimal DeliveryCharge { get; set; }
        public bool IsAlreadyCharged { get; set; }
    }
}
