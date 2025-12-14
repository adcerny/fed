using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;

        public DeliveriesController(IDeliveryService deliveryService)
        {
            _deliveryService =
                deliveryService
                ?? throw new ArgumentNullException(nameof(deliveryService));
        }

        /// <summary>
        /// Returns deliveries for a given date.
        /// </summary>
        /// <param name="date">The date for which deliveries should be returned (e.g. 2019-05-27).</param>
        [HttpGet("/deliveries")]
        public async Task<ActionResult<IList<Order>>> GetDeliveries([FromQuery]DateTime date)
        {
            var result = await _deliveryService.GetDeliveriesAsync(date.ToDate());

            return Ok(result);
        }

        /// <summary>
        /// Returns deliveries for a given date.
        /// </summary>
        /// <param name="deliveryId">The date for which deliveries should be returned (e.g. 2019-05-27).</param>
        [HttpGet("/deliveries/{deliveryId}")]
        public async Task<ActionResult<Delivery>> GetDelivery([FromRoute]string deliveryId)
        {
            var result = await _deliveryService.GetDeliveryAsync(deliveryId);

            return Ok(result);
        }

        /// <summary>
        /// Creates deliveries for a given date.
        /// </summary>
        /// <param name="date">The date for which deliveries should be created (e.g. 2019-05-27).</param>
        [HttpPut("/deliveries/{date}")]
        public async Task<ActionResult<IList<Delivery>>> CreateDeliveries([FromRoute]DateTime date)
        {
            var result = await _deliveryService.CreateDeliveriesAsync(date);
            return Ok(result);
        }

        /// <summary>
        /// Deletes deliveries and associated orders for a given date.
        /// </summary>
        /// <param name="date">The date for which deliveries should be created (e.g. 2019-05-27).</param>
        [HttpDelete("/deliveries/{date}")]
        public async Task<ActionResult<IList<Delivery>>> DeleteDeliveries([FromRoute]DateTime date)
        {
            var result = await _deliveryService.DeleteDeliveriesAsync(date);
            return Ok(result);
        }

        /// <summary>
        /// Sets the packaging status for a given delivery.
        /// </summary>
        /// <param name="deliveryId">The delivery ID for which the status should be set.</param>
        /// <param name="packagingStatusId">The new packaging status to be set on the delivery.</param>
        /// <returns></returns>
        [HttpPut("/deliveries/{deliveryId}/packagingStatus/{packagingStatusId}")]
        public async Task<ActionResult<Delivery>> SetPackagingStatus([FromRoute]Guid deliveryId, int packagingStatusId)
        {
            await _deliveryService.SetDeliveryPackagingStatusAsync(deliveryId, (PackingStatus)packagingStatusId);

            var delivery = await _deliveryService.GetDeliveryAsync(deliveryId.ToString());

            return Ok(delivery);
        }

        /// <summary>
        /// Sets the bag count for a given delivery.
        /// </summary>
        /// <param name="deliveryId">The delivery ID for which the bag count should be set.</param>
        /// <param name="bagCount">The new bag count to be set on the delivery.</param>
        /// <returns></returns>
        [HttpPut("/deliveries/{deliveryId}/bagCount/{bagCount}")]
        public async Task<ActionResult<Delivery>> SetBagCount([FromRoute]string deliveryId, int bagCount)
        {
            await _deliveryService.SetDeliveryBagCountAsync(deliveryId, bagCount);

            var delivery = await _deliveryService.GetDeliveryAsync(deliveryId.ToString());

            return Ok(delivery);
        }

        /// <summary>
        /// Gets delivery shortages for a given date.
        /// </summary>
        /// <param name="date">The delivery date to get shortages for.</param>
        [HttpGet("/deliveryShortages/{date}")]
        public async Task<ActionResult<IList<DeliveryShortage>>> GetDeliveryShortages([FromRoute]DateTime date)
        {
            var result = await _deliveryService.GetDeliveryShortagesAsync(date);

            return Ok(result);
        }

        /// <summary>
        /// Shorts an order item of a given delivery.
        /// </summary>
        /// <param name="deliveryShortage">The details of the delivery shortage.</param>
        [HttpPost("/deliveryShortages")]
        public async Task<ActionResult<DeliveryShortage>> CreateDeliveryShortage(DeliveryShortage deliveryShortage)
        {
            var result = await _deliveryService.ShortDeliveryItemAsync(
                deliveryShortage.OrderId,
                deliveryShortage.ProductId,
                deliveryShortage.ActualQuantity,
                deliveryShortage.ProductPrice,
                deliveryShortage.Reason,
                deliveryShortage.ReasonCode);

            return Ok(result);
        }

        /// <summary>
        /// Deletes an existing delivery shortage record.
        /// </summary>
        /// <param name="deliveryShortageId">The ID of the delivery shortage to be deleted.</param>
        [HttpDelete("/deliveryShortages/{deliveryShortageId}")]
        public async Task<ActionResult> DeleteDeliveryShortage(Guid deliveryShortageId)
        {
            await _deliveryService.DeleteDeliveryShortageAsync(deliveryShortageId);

            return NoContent();
        }

        /// <summary>
        /// Adds a free addition to a given delivery.
        /// </summary>
        /// <param name="deliveryAddition">The details of the delivery addition.</param>
        [HttpPost("/deliveryAdditions")]
        public async Task<ActionResult<DeliveryAddition>> CreateDeliveryAddition(DeliveryAddition deliveryAddition)
        {
            var result = await _deliveryService.AddSubstituteToDeliveryAsync(
                deliveryAddition.OrderId,
                deliveryAddition.ProductId,
                deliveryAddition.Quantity,
                deliveryAddition.Notes,
                deliveryAddition.DeliveryShortageId);

            return Ok(result);
        }

        /// <summary>
        /// Deletes an existing delivery addition.
        /// </summary>
        /// <param name="deliveryAdditionId">The ID of the delivery addition to be deleted.</param>
        [HttpDelete("/deliveryAdditions/{deliveryAdditionId}")]
        public async Task<ActionResult> DeleteDeliveryAddition(Guid deliveryAdditionId)
        {
            await _deliveryService.DeleteDeliveryAdditionAsync(deliveryAdditionId);

            return NoContent();
        }

        /// <summary>
        /// Gets delivery additions for a given date.
        /// </summary>
        /// <param name="date">The delivery date to get additions for.</param>
        [HttpGet("/deliveryAdditions/{date}")]
        public async Task<ActionResult<IList<DeliveryAddition>>> GetDeliveryAdditions([FromRoute]DateTime date)
        {
            var result = await _deliveryService.GetDeliveryAdditionsAsync(date);

            return Ok(result);
        }
    }
}