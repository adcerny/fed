using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class TimeslotsController : ControllerBase
    {
        private readonly ITimeslotsService _timeslotsService;

        public TimeslotsController(ITimeslotsService timeslotsService)
        {
            _timeslotsService = timeslotsService ?? throw new ArgumentNullException(nameof(timeslotsService));
        }

        /// <summary>
        /// Returns a list of timeslots for a given hub.
        /// </summary>
        /// <param name="hubId">The hub ID.</param>
        /// <param name="onlyAvailable">If true only timeslots with an available capacity will be returned.</param>
        [HttpGet("/timeslots")]
        public async Task<ActionResult<IList<Timeslot>>> GetTimeslots(Guid hubId, bool onlyAvailable)
        {
            var timeslots = await _timeslotsService.GetTimeslots(hubId, onlyAvailable);

            if (timeslots == null || timeslots.Count == 0)
                return NotFound($"Unable to return a list of timeslots for hubId {hubId}, because a hub with this ID could not be found.");

            return Ok(timeslots);
        }


        /// <summary>
        /// Returns a list a timeslot with a given id.
        /// </summary>
        /// <param name="timeslotId">The hub ID.</param>
        [HttpGet("/timeslots/{timeslotId}")]
        public async Task<ActionResult<IList<Timeslot>>> GetTimeslot([FromRoute]Guid timeslotId)
        {
            var timeslot = await _timeslotsService.GetTimeslot(timeslotId);

            if (timeslot == null)
                return NotFound($"Timeslot with Id {timeslotId} does not exist.");

            return Ok(timeslot);
        }
    }
}