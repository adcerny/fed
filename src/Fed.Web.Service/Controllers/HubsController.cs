using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class HubsController : ControllerBase
    {
        private readonly IHubsHandler _handler;

        public HubsController(IHubsHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        /// <summary>
        /// Returns a list of all available hubs.
        /// </summary>
        [HttpGet("/hubs")]
        public async Task<ActionResult<IList<Hub>>> GetHubs()
        {
            var query = new GetHubsQuery();
            var result = await _handler.ExecuteAsync(query);
            return Ok(result);
        }

        /// <summary>
        /// Updates a given hub.
        /// </summary>
        /// <param name="id">The hub ID.</param>
        /// <param name="hub">A modified hub object.</param>
        /// <response code="404">Hub not found.</response>
        [HttpPut("/hubs/{id}")]
        public async Task<ActionResult<Hub>> UpdateHub(Guid id, Hub hub)
        {
            var cmd = new UpdateCommand<Hub>(id, hub);
            var updatedHub = await _handler.ExecuteAsync(cmd);

            if (updatedHub == default(Hub))
                return NotFound($"A hub with ID {id} could not be found.");

            return Ok(updatedHub);
        }
    }
}