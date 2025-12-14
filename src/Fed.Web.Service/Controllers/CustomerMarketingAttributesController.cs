using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class CustomerMarketingAttributesController : ControllerBase
    {
        private readonly ICustomerMarketingAttributeService _customerMarketingAttributeService;

        public CustomerMarketingAttributesController(
            ICustomerMarketingAttributeService customerMarketingAttributeService)
        {
            _customerMarketingAttributeService = customerMarketingAttributeService ?? throw new ArgumentNullException(nameof(customerMarketingAttributeService));
        }

        /// <summary>
        /// Returns all customer marketing attributes.
        /// </summary>
        [HttpGet("/customerMarketingAttributes")]
        public async Task<ActionResult<IList<CustomerMarketingAttribute>>> GetCustomerMarketingAttributes()
        {
            var result = await _customerMarketingAttributeService.GetAllCustomerMarketingAttributesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns a customer marketing attribute matching the supplied Id.
        /// </summary>
        /// <param name="id">The customer marketing attribute Id.</param>
        /// <response code="404">Customer not found.</response>
        [HttpGet("/customerMarketingAttributes/{id}")]
        public async Task<ActionResult<CustomerMarketingAttribute>> GetCustomerMarketingAttribute(Guid id)
        {
            try
            {
                var agent = await _customerMarketingAttributeService.GetCustomerMarketingAttributeAsync(id);
                return Ok(agent);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new customer marketing attribute.
        /// </summary>
        /// <response code="400">Validation failed</response>
        [HttpPost("/customerMarketingAttributes")]
        public async Task<ActionResult<CustomerMarketingAttribute>> CreateCustomerMarketingAttribute([FromBody]CustomerMarketingAttribute customerMarketingAttribute)
        {
            var result = await _customerMarketingAttributeService.CreateCustomerMarketingAttributeAsync(customerMarketingAttribute);
            return Ok(result);
        }


        /// <summary>
        /// Updates a customer marketing attribute.
        /// </summary>
        [HttpPatch("/customerMarketingAttributes/{id}")]
        public async Task<ActionResult<bool>> UpdateCustomerMarketingAttribute(Guid id, JsonPatchDocument<CustomerMarketingAttribute> patch)
        {
            var customerMarketingAttribute = await _customerMarketingAttributeService.GetCustomerMarketingAttributeAsync(id);

            if (customerMarketingAttribute == null)
                return NotFound($"No customer marketing attribute found with ID {id}.");

            patch.ApplyTo(customerMarketingAttribute);

            var result = await _customerMarketingAttributeService.UpdateCustomerMarketingAttributeAsync(customerMarketingAttribute);
            return Ok(result);
        }
    }
}