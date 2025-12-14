using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Exceptions;
using Fed.Core.Extensions;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class RecurringOrdersController : ControllerBase
    {
        private readonly IRecurringOrderService _recurringOrdersService;
        private readonly ISkipDatesHandler _skipDatesHandler;

        public RecurringOrdersController(
            IRecurringOrderService recurringOrdersService,
            ISkipDatesHandler skipDatesHandler)
        {
            _recurringOrdersService =
                recurringOrdersService
                ?? throw new ArgumentNullException(nameof(recurringOrdersService));

            _skipDatesHandler =
                skipDatesHandler
                ?? throw new ArgumentNullException(nameof(skipDatesHandler));
        }

        /// <summary>
        /// Returns a list of recurring orders for a given contact.
        /// </summary>
        /// <param name="contactId">The contact ID.</param>
        /// <param name="fromDate">Show only recurring orders which fall in a given time range.</param>
        /// <param name="toDate">Show only recurring orders which fall in a given time range.</param>
        /// <param name="includeOrdersFromDemoAccounts">If true includes orders from demo accounts.</param>
        [HttpGet("/recurringorders")]
        public async Task<ActionResult<IList<RecurringOrder>>> GetRecurringOrders(
            Guid contactId,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool includeOrdersFromDemoAccounts = false)
        {
            var result = await _recurringOrdersService.GetRecurringOrdersAsync(
                contactId,
                DateRange.Create(fromDate, toDate),
                includeOrdersFromDemoAccounts);

            return Ok(result);
        }

        /// <summary>
        /// Gets a recurring order by ID.
        /// </summary>
        /// <param name="id">The recurring order ID.</param>
        /// <response code="404"> Recurring order not found.</response>
        [HttpGet("/recurringorders/{id}")]
        public async Task<ActionResult<RecurringOrder>> GetRecurringOrder(Guid id)
        {
            var result = await _recurringOrdersService.GetRecurringOrderAsync(id);

            if (result == null)
                return NotFound($"No recurring order with ID {id} could be found.");

            return Ok(result);
        }

        /// <summary>
        /// Creates a new recurring order.
        /// </summary>
        /// <param name="recurringOrder">The recurring order object to be created.</param>
        [HttpPost("/recurringorders")]
        public async Task<ActionResult<RecurringOrder>> CreateRecurringOrder(RecurringOrder recurringOrder)
        {
            try
            {
                var result = await _recurringOrdersService.CreateAsync(recurringOrder);
                return Ok(result);
            }
            catch(PastCutOffException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Updates a given recurring order.
        /// </summary>
        /// <param name="id">The recurring order ID to be updated.</param>
        /// <param name="recurringOrder">An updated recurring order object.</param>
        [HttpPut("/recurringorders/{id}")]
        public async Task<ActionResult<RecurringOrder>> UpdateRecurringOrder(Guid id, RecurringOrder recurringOrder)
        {
            var result = await _recurringOrdersService.UpdateAsync(id, recurringOrder);
            return Ok(result);
        }

        /// <summary>
        /// Updates a given recurring for a given date only.
        /// </summary>
        /// <param name="id">The recurring order ID to be updated.</param>
        /// <param name="date">The date to be changed (e.g. 2019-05-28).</param>
        /// <param name="recurringOrder">An updated recurring order object.</param>
        [HttpPut("/recurringorders/{id}/{date}")]
        public async Task<ActionResult<RecurringOrder>> UpdateRecurringOrder(
            [FromRoute]Guid id,
            [FromRoute]DateTime date,
            [FromBody]RecurringOrder recurringOrder)
        {
            var updatedOrder = await _recurringOrdersService.UpdateForSingleDateAsync(id, date.ToDate(), recurringOrder);
            return Ok(updatedOrder);
        }

        /// <summary>
        /// Updates a given recurring from a given date onwards.
        /// </summary>
        /// <param name="id">The recurring order ID to be updated.</param>
        /// <param name="date">The date to be skipped (e.g. 2019-05-28).</param>
        /// <param name="recurringOrder">An updated recurring order object.</param>
        [HttpPost("/recurringorders/{id}/{date}")]
        public async Task<ActionResult<RecurringOrder>> UpdateRecurringOrderFromDate([FromRoute]Guid id, [FromRoute]DateTime date, [FromBody]RecurringOrder recurringOrder)
        {
            var updatedOrder = await _recurringOrdersService.UpdateFromDateAsync(id, date.ToDate(), recurringOrder);
            return Ok(updatedOrder);
        }

        /// <summary>
        /// Updates only selected properties of a given recurring order.
        /// </summary>
        /// <param name="id">The recurring order ID to be updated.</param>
        /// <param name="patch">A list of patch operations.</param>
        [HttpPatch("/recurringorders/{id}")]
        public async Task<ActionResult<RecurringOrder>> PatchRecurringOrder(Guid id, JsonPatchDocument<RecurringOrder> patch)
        {
            var recurringOrder = await _recurringOrdersService.GetRecurringOrderAsync(id);

            if (recurringOrder == null)
                return NotFound($"No recurring order with ID {id} could be found.");

            patch.ApplyTo(recurringOrder);

            var result = await _recurringOrdersService.UpdateAsync(id, recurringOrder);

            return Ok(result);
        }

        /// <summary>
        /// Deletes a recurring order.
        /// </summary>
        /// <param name="id">The recurring order ID to be deleted.</param>
        /// /// <response code="404"> Recurring order not found.</response>
        [HttpDelete("/recurringorders/{id}")]
        public async Task<ActionResult> DeleteRecurringOrder(Guid id)
        {
            await _recurringOrdersService.DeleteAsync(id);

            return Ok(true);
        }

        /// <summary>
        /// Gets all skipped dates for a given recurring order.
        /// </summary>
        /// <param name="id">The recurring order ID.</param>
        /// <response code="404"> Recurring order not found.</response>
        [HttpGet("/recurringorders/{id}/skipdates")]
        public async Task<ActionResult<IList<SkipDate>>> GetSkipDates(Guid id)
        {
            var query = new GetSkipDatesQuery(id, null);
            var result = await _skipDatesHandler.ExecuteAsync(query);

            if (result == null)
                return NotFound($"No recurring order with ID {id} could be found.");

            return Ok(result);
        }

        /// <summary>
        /// Skips a given date for a given recurring order.
        /// </summary>
        /// <param name="id">The recurring order ID.</param>
        /// <param name="date">The date to be skipped (e.g. 2019-05-28).</param>
        /// <param name="skipDateReason">An object specifying the reason for skipping an order and the person who has instructed the skip date.</param>
        [HttpPut("/recurringorders/{id}/skipdates/{date}")]
        public async Task<ActionResult<IList<SkipDate>>> CreateSkipDate([FromRoute]Guid id, [FromRoute]DateTime date, [FromBody]SkipDateReason skipDateReason)
        {
            var query = new GetSkipDatesQuery(id, DateRange.SingleDay(date));
            var result = await _skipDatesHandler.ExecuteAsync(query);

            var skipDay = date.ToDate();

            // If the date is already skipped then return the existing list
            if (result != null && result.Count > 0)
                return Ok(result.First().Date);

            var cmd = new CreateSkipDateCommand(id, skipDay, skipDateReason.Reason, skipDateReason.CreatedBy);
            var updatedResult = await _skipDatesHandler.ExecuteAsync(cmd);
            return Ok(updatedResult);
        }

        /// <summary>
        /// Deletes a given skip date for a recurring order (meaning the order will get carried out if nothing else changes).
        /// </summary>
        /// <param name="id">The recurring order ID.</param>
        /// <param name="date">A previously skipped date to be removed now.</param>
        /// <response code="404"> Recurring order not found.</response>
        [HttpDelete("/recurringorders/{id}/skipdates/{date}")]
        public async Task<ActionResult<IList<SkipDate>>> DeleteSkipDate([FromRoute]Guid id, [FromRoute]DateTime date)
        {
            var query = new GetSkipDatesQuery(id, DateRange.SingleDay(date));
            var result = await _skipDatesHandler.ExecuteAsync(query);

            if (result == null)
                return NotFound($"No skip date with recurring order with ID {id} could be found.");

            var skipDay = date.ToDate();

            var cmd = new DeleteSkipDateCommand(id, skipDay);
            var updatedResult = await _skipDatesHandler.ExecuteAsync(cmd);
            return Ok(updatedResult);
        }


        /// <summary>
        /// Gets a recurring order for a date with delivery charge and discount info.
        /// </summary>
        /// <param name="id">The recurring order ID.</param>
        /// <param name="date">The delivery date of the order to return.</param>
        /// <response code="404"> Recurring order not found.</response>
        [HttpGet("/recurringorders/{id}/{date}")]
        public async Task<ActionResult<OrderDeliveryContext<RecurringOrder>>> GetRecurringOrderForDate([FromRoute]Guid id, [FromRoute]DateTime date)
        {
            var result = await _recurringOrdersService.GetRecurringOrderForDateAsync(id, date);

            if (result == null)
                return NotFound($"No recurring order with ID {id} could be found.");

            return Ok(result);
        }

        /// <summary>
        /// Gets a delivery charge for a recurring order on a date and time .
        /// </summary>
        /// <param name="customerId">The Id of the customer placing the order.</param>
        /// <param name="timeslotId">The Id of the timeslot to check.</param>
        /// <param name="recurringOrderId">The recurring order ID. Null for a new order</param>
        /// <param name="deliveryDate">The date that the order is due to be delivered.</param>
        /// <response code="404"> Recurring order not found.</response>
        [HttpGet("/recurringorders/deliverycharge")]
        public async Task<ActionResult<DeliveryChargeResult>> GetDeliveryChargeForTimeslotDate([FromQuery]Guid customerId, [FromQuery]Guid timeslotId, [FromQuery]Guid? recurringOrderId = null, [FromQuery]DateTime? deliveryDate = null)
        {
            var result = await _recurringOrdersService.GetDeliveryChargeForTimeslotDateAsync(customerId, timeslotId, recurringOrderId, deliveryDate);
            return Ok(result);
        }
    }
}