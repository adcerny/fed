using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class CustomersAgentsController : ControllerBase
    {
        private readonly ICustomerAgentService _customerAgentService;

        public CustomersAgentsController(
            ICustomerAgentService customerAgentService)
        {
            _customerAgentService = customerAgentService ?? throw new ArgumentNullException(nameof(customerAgentService));
        }

        /// <summary>
        /// Returns all customer agents.
        /// </summary>
        [HttpGet("/customerAgents")]
        public async Task<ActionResult<IList<CustomerAgent>>> GetCustomerAgents()
        {
            var result = await _customerAgentService.GetAllCustomerAgentsAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns a customer agent matching the supplied Id.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <response code="404">Customer not found.</response>
        [HttpGet("/customerAgents/{id}")]
        public async Task<ActionResult<CustomerAgent>> GetCustomerAgent(Guid id)
        {
            try
            {
                var agent = await _customerAgentService.GetCustomerAgentAsync(id);
                return Ok(agent);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new customer agent.
        /// </summary>
        /// <response code="400">Validation failed</response>
        [HttpPost("/customerAgents")]
        public async Task<ActionResult<CustomerAgent>> CreateCustomerAgent([FromBody]CustomerAgent customerAgent)
        {
            var result = await _customerAgentService.CreateCustomerAgentAsync(customerAgent);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a customer agent.
        /// </summary>
        /// <response code="400">Validation failed</response>
        [HttpDelete("/customerAgents/{id}")]
        public async Task<ActionResult<bool>> DeleteCustomerAgent([FromRoute]Guid id)
        {
            var result = await _customerAgentService.DeleteCustomerAgentAsync(id);
            return Ok(result);
        }
    }
}