using Fed.Core.Enums;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Models
{
    public class ForecastedDeliveries
    {

        public ForecastedDeliveries()
        {
            DeliveryAddressIds = new List<Guid>();
        }

        public Guid TimeslotId { get; set; }  
        public IList<Guid> DeliveryAddressIds { get; set; }
    }
}
