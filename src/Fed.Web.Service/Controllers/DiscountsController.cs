using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class DiscountsController : ControllerBase
    {
        private readonly IDiscountService _discountService;

        public DiscountsController(IDiscountService discountService, IOrderService orderService)
        {
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        }

        /// <summary>
        /// Returns an discount matching the supplied Id.
        /// </summary>
        /// <param name="id">The discount Id.</param>
        /// <response code="404">Discount not found</response>
        [HttpGet("/discounts/{id}")]
        public async Task<ActionResult<Discount>> GetDiscount(Guid id)
        {
            var discount = await _discountService.GetDiscount(id);

            if (discount == null)
                return NotFound($"No discount with an ID of {id} exists.");

            return Ok(discount);
        }

        /// <summary>
        /// Gets a list of discounts for a given customer.
        /// </summary>
        /// <param name="customerId">The Id of the customer to get discounts for.</param>
        [HttpGet("/discounts")]
        public async Task<ActionResult<IList<Discount>>> GetDiscounts([FromQuery]Guid? customerId = null)
        {
            var discounts = await _discountService.GetDiscounts(customerId);
            return Ok(discounts);
        }

        /// <summary>
        /// Calculates the discounts that a customer is eligible for a given order.
        /// </summary>
        /// <param name="query">The query containing information required by the calculation.</param>
        [HttpPost("/discounts/calculate")]
        public async Task<ActionResult<IList<DiscountResult>>> CalculateDiscount([FromBody]DiscountCalculationQuery query)
        {
            var discounts = await _discountService.CalculateDiscount(query);
            return Ok(discounts);
        }

        /// <summary>
        /// Creates an discount
        /// </summary>
        [HttpPost("/discounts")]
        public async Task<ActionResult<Guid>> CreateDiscount([FromBody]Discount discount)
        {
            var id = await _discountService.CreateDiscount(discount);
            return Ok(id);
        }

        /// <summary>
        /// Partially updates an existing discount.
        /// </summary>
        /// <param name="id">The discount Id.</param>
        /// <param name="patch">The patch operation.</param>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Discount not found</response>
        [HttpPatch("/discounts/{id}")]
        public async Task<ActionResult<Discount>> PatchDiscount(Guid id, JsonPatchDocument<Discount> patch)
        {
            var discount = await _discountService.GetDiscount(id);
            if (discount == null)
                return NotFound($"No discount with an ID of {id} exists.");

            patch.ApplyTo(discount);

            Discount result = await _discountService.UpdateDiscount(id, discount);
            return Ok(result);
        }

        /// <summary>
        /// Updates an existing discount.
        /// </summary>
        /// <param name="id">The discount Id.</param>
        /// <param name="discount">The discount to update.</param>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Discount not found</response>
        [HttpPut("/discounts/{id}")]
        public async Task<ActionResult<Discount>> PutDiscount(Guid id, Discount discount)
        {
            var existingDiscount = await _discountService.GetDiscount(id);
            if (existingDiscount == null)
                return NotFound($"No discount with an ID of {id} exists.");

            Discount result = await _discountService.UpdateDiscount(id, discount);
            return Ok(result);
        }

        /// <summary>
        /// Applies a discount
        /// </summary>
        [HttpPut("/discounts/customers/{customerId}")]
        public async Task<ActionResult<Guid>> ApplyDiscounts([FromRoute]Guid customerId, [FromQuery]DiscountEvent discountEvent)
        {
            var id = await _discountService.ApplyDiscounts(customerId, discountEvent);
            return Ok(id);
        }

        /// <summary>
        /// Applies a discount
        /// </summary>
        [HttpPut("/discounts/{discountId}/{customerId}")]
        public async Task<ActionResult<bool>> ApplyDiscount([FromRoute]Guid discountId, [FromRoute]Guid customerId)
        {
            var result = await _discountService.ApplyDiscount(discountId, customerId);
            return Ok(result);
        }

        /// <summary>
        /// Creates a discount code for a given discount.
        /// </summary>
        /// <param name="id">The discount Id.</param>
        /// <param name="code">The discount code to add.</param>
        /// <response code="404">Discount not found</response>
        /// <response code="409">Discount code already exists</response>
        [HttpPost("/discounts/{id}/discountCode")]
        public async Task<ActionResult<Discount>> AddDiscountCode([FromRoute]Guid id, [FromBody]DiscountCode code)
        {
            var discount = await _discountService.GetDiscount(id);
            if (discount == null)
                return NotFound($"No discount with an ID of {id} exists.");

            try
            {
                var result = await _discountService.AddDiscountCode(id, code);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Applies a discount code
        /// </summary>
        /// <param name="code">The discount code to add.</param>
        /// <param name="customerId">The Id of the customer to apply the discount to.</param>
        ///<response code="409">Discount code cannot be applied</response>
        [HttpPut("/discountCodes/{code}")]
        public async Task<ActionResult<Discount>> ApplyDiscountCode([FromRoute]string code, [FromQuery]Guid? customerId)
        {
            try
            {
                var discount = await _discountService.ApplyDiscountCode(code, customerId);
                return Ok(discount);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}