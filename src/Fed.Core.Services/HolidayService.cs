using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Extensions;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Core.Services
{


    public class HolidayService : IHolidayService
    {
        private readonly IHolidaysHandler _holidaysHandler;

        public HolidayService(IHolidaysHandler holidaysHandler)
        {
            _holidaysHandler = holidaysHandler;
        }

        public async Task<IList<Holiday>> GetHolidaysAsync(DateRange dateRange)
        {
            return await _holidaysHandler.ExecuteAsync(new GetHolidaysQuery(dateRange));
        }

        public async Task<bool> CreateHolidayAsync(Holiday holiday)
        {
            var existingHoliday = await GetHolidaysAsync(DateRange.SingleDay(holiday.Date));
            if (existingHoliday.OrEmptyIfNull().Count() > 0)
                throw new ArgumentException($"A holiday already exists on {holiday.Date}");

            return await _holidaysHandler.ExecuteAsync(new CreateCommand<Holiday>(holiday));
        }

        public async Task<bool> DeleteHolidayAsync(Date date)
        {
            var existingHoliday = await GetHolidaysAsync(DateRange.SingleDay(date));
            if (existingHoliday.OrEmptyIfNull().Count() == 0)
                throw new ArgumentException($"No holiday exists on {date}");

            return await _holidaysHandler.ExecuteAsync(new DeleteHolidayCommand(date));
        }

    }
}
