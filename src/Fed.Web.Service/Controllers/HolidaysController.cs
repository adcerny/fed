using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    public class HolidaysController : ControllerBase
    {
        private readonly IHolidayService _holidayService;

        public HolidaysController(
            IHolidayService holidayService)
        {
            _holidayService = holidayService ?? throw new ArgumentNullException(nameof(holidayService));
        }

        /// <summary>
        /// Returns a list of holidays for a given time range.
        /// </summary>
        /// <param name="startDate">The earliest date to return.</param>
        /// <param name="endDate">The latest date to return.</param>
        [HttpGet("/holidays")]
        public async Task<ActionResult<IList<Holiday>>> GetHolidays(DateTime startDate, DateTime endDate)
        {
            var holidays = await _holidayService.GetHolidaysAsync(DateRange.Create(startDate, endDate));
            return Ok(holidays);
        }

        /// <summary>
        /// Returns a list of holidays for a given time range.
        /// </summary>
        /// <param name="holiday">The earliest date to return.</param>
        ///<response code="409">Holiday already exists on given date</response>
        [HttpPost("/holidays")]
        public async Task<ActionResult<bool>> CreateHoliday([FromBody]Holiday holiday)
        {
            try
            {
                var result = await _holidayService.CreateHolidayAsync(holiday);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Conflict(ex.Message);
            }
        }

        ///<summary>
        /// Returns a list of holidays for a given time range.
        ///</summary>
        ///<param name="holidayDate">The holiday date to delete</param>
        ///<response code="409">Holiday does not exist on given date</response>
        [HttpDelete("/holidays/{holidayDate}")]
        public async Task<ActionResult<bool>> DeleteHoliday([FromRoute]DateTime holidayDate)
        {
            try
            {
                var result = await _holidayService.DeleteHolidayAsync(holidayDate);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Conflict(ex.Message);
            }
        }

    }
}
