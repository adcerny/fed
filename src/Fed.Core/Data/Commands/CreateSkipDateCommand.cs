using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Commands
{
    public class CreateSkipDateCommand : IDataOperation<IList<Date>>
    {
        public CreateSkipDateCommand(Guid recurringOrderId, Date date, string reason, string createdBy)
        {
            RecurringOrderId = recurringOrderId;
            Date = date;
            Reason = reason;
            CreatedBy = createdBy;
            CreatedDateTime = DateTime.UtcNow;
        }

        public Guid RecurringOrderId { get; }
        public Date Date { get; }
        public string Reason { get; }
        public string CreatedBy { get; }
        public DateTime CreatedDateTime { get; }
    }
}