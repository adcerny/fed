using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetOrdersQuery : IDataOperation<IList<Order>>
    {
        public GetOrdersQuery(DateRange deliveryDateRange, Guid? contactId = null, bool excludeUnpaid = false, bool excludeInvoiced = false)
        {
            DeliveryDateRange = deliveryDateRange;
            ContactId = contactId;
            ExcludeUnpaid = excludeUnpaid;
            ExcludeInvoiced = excludeInvoiced;
        }

        public DateRange DeliveryDateRange { get; }
        public Guid? ContactId { get; }
        public bool ExcludeUnpaid { get; }
        public bool ExcludeInvoiced { get; }
    }
}