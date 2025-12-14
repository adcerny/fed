using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface ITimeslotsService
    {
        Task<IList<Timeslot>> GetTimeslots(Guid hubId, bool onlyAvailable, bool allHubs = false);

        Task<Timeslot> GetTimeslot(Guid timeslotId);
    }
}
