using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetDeliveryIdsForPaymentQuery : IDataOperation<IList<Guid>>
    {
        public GetDeliveryIdsForPaymentQuery(Date deliveryDate)
        {
            DeliveryDate = deliveryDate;
        }

        public Date DeliveryDate { get; }
    }
}