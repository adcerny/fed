using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IHolidayService
    {
        Task<IList<Holiday>> GetHolidaysAsync(DateRange dateRange);
        Task<bool> CreateHolidayAsync(Holiday holiday);
        Task<bool> DeleteHolidayAsync(Date date);
    }
}