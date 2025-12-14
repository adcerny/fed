using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
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
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;

        public ProductsController(IProductsService productsService)
        {
            _productsService = productsService ?? throw new ArgumentNullException(nameof(productsService));
        }

        /// <summary>
        /// Returns a list of products (optionally for a given category).
        /// </summary>
        /// <param name="productGroup">An optional product group.</param>
        /// <param name="productCategoryId">An optional category ID.</param>
        [HttpGet("/products")]
        public async Task<ActionResult<IList<Product>>> GetProducts(string productGroup, Guid? productCategoryId = null)
        {
            var products = await _productsService.GetProducts(productGroup, productCategoryId);
            return Ok(products);
        }

        /// <summary>
        /// Returns a list of products (optionally for a given category).
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        [HttpGet("/products/{id}")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            try
            {
                var product = await _productsService.GetProduct(id);
                return Ok(product);
            }
            catch(KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <response code="400">Validation failed</response>
        [HttpPost("/products/")]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            var result = await _productsService.CreateProduct(product);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a product.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        [HttpDelete("/products/{id}")]
        public async Task<ActionResult<bool>> DeleteProduct(string id)
        {
            try
            {
                var result = await _productsService.DeleteProduct(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }

        /// <summary>
        /// Updates a product.
        /// </summary>
        /// <param name="id">The ID of the product.</param>
        /// <param name="patch">The patch operation.</param>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Customer not found</response>
        [HttpPatch("/products/{id}")]
        public async Task<ActionResult> UpdateProduct(string id, JsonPatchDocument<Product> patch)
        {
            try
            {
                await _productsService.PatchProduct(id, patch);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }
    }
}