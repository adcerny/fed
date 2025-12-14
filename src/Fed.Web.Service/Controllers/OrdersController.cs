using Fed.Core.Entities;
using Fed.Core.Extensions;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService =
                orderService
                ?? throw new ArgumentNullException(nameof(orderService));
        }

        /// <summary>
        /// Returns sales orders for a given date.
        /// </summary>
        /// <param name="date">The date for which sales orders should be returned (e.g. 2019-05-27).</param>
        /// <param name="contactId">Optional contact ID to filter orders by contact.</param>
        /// <param name="excludeUnpaid">Optional bool to display orders where the card payment is selected and payments have not succeeded.</param>
        [HttpGet("/orders")]
        public async Task<ActionResult<IList<Order>>> GetOrders([FromQuery]DateTime date, [FromQuery]Guid? contactId = null, [FromQuery]bool excludeUnpaid = false)
        {
            var result = await _orderService.GetOrdersAsync(date.ToDate(), contactId, excludeUnpaid);

            return Ok(result);
        }

        /// <summary>
        /// Returns sales order matching a given ID.
        /// </summary>
        /// <param name="Id">The ID of the order.</param>
        [HttpGet("/orders/{Id}")]
        public async Task<ActionResult<Order>> GetOrder([FromRoute]Guid Id)
        {
            var result = await _orderService.GetOrderAsync(Id);

            return Ok(result);
        }

        /// <summary>
        /// Creates sales orders for a given date.
        /// </summary>
        /// <param name="date">The date for which sales orders should be created (e.g. 2019-05-27).</param>
        [HttpPut("/orders/{date}")]
        public async Task<ActionResult<IList<GeneratedOrder>>> PlaceOrders([FromRoute]DateTime date)
        {
            var result = await _orderService.GenerateOrdersAsync(date.ToDate());

            return Ok(result);
        }

        /// <summary>
        /// Gets an order summary of past and future orders for a given contact id.
        /// </summary>
        /// <param name="contactId">The ID of the contact.</param>
        /// <param name="fromDate">From date for the order summary.</param>
        /// <param name="toDate">To date for the order summary.</param>
        /// <returns></returns>
        [HttpGet("/orders/{contactId}/summary")]
        public async Task<ActionResult<IList<OrderSummary>>> GetOrderSummary([FromRoute]Guid contactId, [FromQuery]DateTime fromDate, [FromQuery]DateTime toDate)
        {
            var orderSummary = await _orderService.GetOrderSummaryAsync(Date.Create(fromDate), Date.Create(toDate), contactId);

            return Ok(orderSummary);
        }

        /// <summary>
        /// Gets an order for a date with delivery charge info.
        /// </summary>
        /// <param name="id">The order ID.</param>
        /// <param name="date">The delivery date of the order to return.</param>
        /// <response code="404"> Recurring order not found.</response>
        [HttpGet("/orders/{id}/{date}")]
        public async Task<ActionResult<OrderDeliveryContext<Order>>> GetOrderForDate([FromRoute]Guid id, [FromRoute]DateTime date)
        {
            var result = await _orderService.GetOrderForDateAsync(id, date);

            if (result == null)
                return NotFound($"No order with ID {id} could be found.");

            return Ok(result);
        }
    }
}