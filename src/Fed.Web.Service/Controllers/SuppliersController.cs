using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class SuppliersController : ControllerBase
    {
        private readonly ISuppliersService _suppliersService;

        public SuppliersController(ISuppliersService suppliersService)
        {
            _suppliersService = suppliersService ?? throw new ArgumentNullException(nameof(suppliersService));
        }

        /// <summary>
        /// Returns a list of suppliers.
        /// </summary>
        [HttpGet("/suppliers")]
        public async Task<ActionResult<IList<Supplier>>> GetSuppliers()
        {
            var result = await _suppliersService.GetSuppliersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns a list of suppliers.
        /// </summary>
        /// <param name="id">The supplier Id.</param>
        /// <response code="404">Supplier not found.</response>
        [HttpGet("/suppliers/{id}")]
        public async Task<ActionResult<Supplier>> GetSupplier([FromRoute]int id)
        {
            var supplier = await _suppliersService.GetSupplierAsync(id);

            if (supplier == null)
                return NotFound($"No supplier found with ID {id}.");

            return Ok(supplier);
        }


        /// <summary>
        /// Creates a new supplier.
        /// </summary>
        [HttpPost("/suppliers")]
        public async Task<ActionResult<Supplier>> CreateSupplier(Supplier supplier)
        {
            var result = await _suppliersService.CreateSupplierAsync(supplier);
            return Ok(result);
        }

        /// <summary>
        /// Updates a supplier.
        /// </summary>
        [HttpPatch("/suppliers/{id}")]
        public async Task<ActionResult<Supplier>> UpdateSupplier(int id, JsonPatchDocument<Supplier> patch)
        {
            var supplier = await _suppliersService.GetSupplierAsync(id);

            if (supplier == null)
                return NotFound($"No supplier found with ID {id}.");

            patch.ApplyTo(supplier);

            var result = await _suppliersService.UpdateSupplierAsync(id, supplier);
            return Ok(result);
        }

        /// <summary>
        /// Returns a list of product data and confirmed quantities for a given date.
        /// </summary>
        /// <param name="supplierId">The supplier ID for which confirmed product quantities should be returned.</param>
        /// <param name="date">The delivery date for which product quantities have been confirmed.</param>
        [HttpGet("/suppliers/{supplierId}/{date}/products")]
        public async Task<ActionResult<IList<SupplierProductQuantity>>> GetConfirmedSupplierQuantities(
            [FromRoute]int supplierId,
            [FromRoute]DateTime date)
        {
            var result = await _suppliersService.GetConfirmedSupplierQuantitiesAsync(supplierId, date);
            return Ok(result);
        }

        /// <summary>
        /// Returns a dictionary of dates and product data with forecast quantities per product and day.
        /// </summary>
        /// <param name="supplierId">The supplier ID for which the forecast should be generated.</param>
        /// <param name="toDate">The to-date until which the forecast should be generated.</param>
        /// <returns></returns>
        [HttpGet("/suppliers/{supplierId}/forecast")]
        public async Task<ActionResult<IDictionary<DateTime, IList<SupplierProductQuantity>>>> GetSupplierForecast(
            [FromRoute]int supplierId,
            [FromQuery]DateTime toDate)
        {
            var result = await _suppliersService.GetSupplierForecastAsync(supplierId, Date.Create(toDate));
            return Ok(result);
        }

    }
}