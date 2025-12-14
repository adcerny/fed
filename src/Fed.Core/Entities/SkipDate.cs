using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using System;

namespace Fed.Core.Entities
{
    public class SkipDate : SkipDateReason
    {
        public SkipDate(
            Guid recurringOrderId,
            DateTime date,
            string reason,
            string createdBy,
            DateTime createdDateTime)
        {
            RecurringOrderId = recurringOrderId;
            Date = date.ToDate();
            Reason = reason;
            CreatedBy = createdBy;
            CreatedDateTime = createdDateTime;
        }

        public Guid RecurringOrderId { get; }
        public Date Date { get; }
        public DateTime CreatedDateTime { get; }
    }
}