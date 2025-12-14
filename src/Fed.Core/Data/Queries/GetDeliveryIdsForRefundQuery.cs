using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetDeliveryAndTrasactionIdsForRefundQuery : IDataOperation<IList<(Guid, Guid)>>
    {
        public GetDeliveryAndTrasactionIdsForRefundQuery(Date deliveryDate)
        {
            DeliveryDate = deliveryDate;
        }

        public Date DeliveryDate { get; }
    }
}