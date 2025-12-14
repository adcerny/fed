using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class PostcodesController : ControllerBase
    {
        private readonly IPostcodeHubService _postcodeDeliverableService;
        private readonly IPostcodeAddressesService _postcodesAddressesService;
        private readonly IPostcodeLocationHandler _postcodeLocationHandler;

        public PostcodesController(
            IPostcodeHubService postcodeService,
            IPostcodeAddressesService postcodesAddressesService,
            IPostcodeLocationHandler postcodeLocationHandler)
        {
            _postcodeDeliverableService = postcodeService;
            _postcodesAddressesService = postcodesAddressesService;
            _postcodeLocationHandler = postcodeLocationHandler;
        }

        /// <summary>
        /// Finds the hub which delivers to a given postcode. If no hub delivers to a given postcode then an empty string will be returned.
        /// </summary>
        /// <param name="postcode">The delivery postcode.</param>
        /// <response code="200">Valid postcode.  Hub Id returned if deliverable. Empty string returned if not deliverable.</response>
        /// <response code="404">Postcode not found</response>
        /// <response code="400">Invalid postcode</response>
        [HttpGet("/postcodes/{postcode}/hubId")]
        public async Task<ActionResult<string>> Postcode(string postcode)
        {
            try
            {
                Guid hubId = await _postcodeDeliverableService.GetHubIdForPostcode(postcode);
                return new JsonResult(hubId == Guid.Empty ? string.Empty : hubId.ToString());
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Returns a list of addresses which are associated with a given postcode that we deliver to.  If we do not deliver to the postcode, no addresses will be returned.
        /// </summary>
        /// <param name="postcode">The delivery postcode.</param>
        /// <response code="200">Valid postcode. Addresses returned if deliverable. No addresses returned if not deliverable.</response>
        /// <response code="400">Invalid postcode</response>
        /// <response code="404">Postcode not found</response>
        [HttpGet("/postcodes/{postcode}/deliverableAddresses")]
        public async Task<ActionResult<List<DeliverableAddress>>> GetDeliverableAddresses(string postcode)
        {
            try
            {
                var addresses = await _postcodesAddressesService.GetDeliverableAddresses(postcode);
                return Ok(addresses);
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Returns a list of addresses which are associated with a given postcode.
        /// </summary>
        /// <param name="postcode">The postcode.</param>
        /// <response code="200">Valid postcode. List of addresses returned.</response>
        /// <response code="400">Invalid postcode</response>
        /// <response code="404">Postcode not found</response>
        [HttpGet("/postcodes/{postcode}/addresses")]
        public async Task<ActionResult<List<DeliverableAddress>>> GetAddresses(string postcode)
        {
            try
            {
                var addresses = await _postcodesAddressesService.GetAddresses(postcode);
                return Ok(addresses);
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return new BadRequestObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Adds an email address to be contacted if postcode becomes deliverable in the future.
        /// </summary>
        /// <param name="postcode">The postcode.</param>
        /// <param name="email">The postcode.</param>
        /// <response code="200">Email address successfully added</response>
        [HttpPost("/postcodes/{postcode}/addEmail")]
        public async Task<ActionResult<bool>> AddEmail([FromRoute]string postcode, string email)
        {

            Guid hubId = await _postcodeDeliverableService.GetHubIdForPostcode(postcode);
            bool isDeliverable = !hubId.Equals(Guid.Empty);
            await _postcodeLocationHandler.ExecuteAsync(
                new CreateCommand<PostcodeContact>(
                    new PostcodeContact
                    {
                        Postcode = Address.NormalisePostcode(postcode),
                        Email = email,
                        IsDeliverable = isDeliverable,
                        DateAdded = DateTime.UtcNow
                    }));
            return Ok(true);
        }
    }
};