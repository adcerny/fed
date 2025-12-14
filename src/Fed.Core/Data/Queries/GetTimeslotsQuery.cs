using Fed.Core.Entities;
using System;
using System.Collections.Generic;

namespace Fed.Core.Data.Queries
{
    public class GetTimeslotsQuery : IDataOperation<IList<Timeslot>>
    {
        public GetTimeslotsQuery(Guid hubId, bool onlyAvailableTimeslots, bool allHubs = false)
        {
            HubId = hubId;
            OnlyAvailableTimeslots = onlyAvailableTimeslots;
            AllHubs = allHubs;
        }

        public Guid HubId { get; }
        public bool OnlyAvailableTimeslots { get; }
        public bool AllHubs { get; }
    }
}