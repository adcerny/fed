using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetRecurringOrdersQuery : IDataOperation<IList<RecurringOrder>>
    {
        public GetRecurringOrdersQuery(
            DateRange dateRange,
            Guid? contactId = null,
            bool includeExpired = false,
            bool includeFromDeletedAccounts = false,
            bool includeFromCancelledAccounts = false,
            bool includeFromDemoAccounts = false,
            bool includeFromPausedAccounts = false)
        {
            DateRange = dateRange;
            ContactId = contactId;
            IncludeExpired = includeExpired;
            IncludeFromDeletedAccounts = includeFromDeletedAccounts;
            IncludeFromCancelledAccounts = includeFromCancelledAccounts;
            IncludeFromDemoAccounts = includeFromDemoAccounts;
            IncludeFromPausedAccounts = includeFromPausedAccounts;
        }

        public DateRange DateRange { get; }
        public Guid? ContactId { get; }
        public bool IncludeExpired { get; }
        public bool IncludeFromDeletedAccounts { get; }
        public bool IncludeFromCancelledAccounts { get; }
        public bool IncludeFromDemoAccounts { get; }
        public bool IncludeFromPausedAccounts { get; }
    }
}