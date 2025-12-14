using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Commands
{
    public class DeleteSkipDateCommand : IDataOperation<IList<Date>>
    {
        public DeleteSkipDateCommand(Guid recurringOrderId, Date date)
        {
            RecurringOrderId = recurringOrderId;
            Date = date;
        }

        public Guid RecurringOrderId { get; }
        public Date Date { get; }
    }
}
