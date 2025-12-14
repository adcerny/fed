using Fed.Api.External.AzureStorage;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.Services.Validators;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class ProductCategoriesController : ControllerBase
    {
        private readonly IProductCategoryService _productCategoryService;

        public ProductCategoriesController(
            IProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService ?? throw new ArgumentNullException(nameof(productCategoryService));
        }

        /// <summary>
        /// Returns all product categories.
        /// </summary>
        [HttpGet("/ProductCategories")]
        public async Task<ActionResult<IList<ProductCategory>>> GetProductCategories()
        {
            var result = await _productCategoryService.GetAllProductCategoriesAsync();
            return Ok(result);
        }

        /// <summary>
        /// Returns a product category matching the supplied Id.
        /// </summary>
        /// <param name="id">The product category Id.</param>
        /// <response code="404">Product category not found.</response>
        [HttpGet("/ProductCategories/{id}")]
        public async Task<ActionResult<ProductCategory>> GetProductCategory(Guid id)
        {
            try
            {
                var agent = await _productCategoryService.GetProductCategoryAsync(id);
                return Ok(agent);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new product category.
        /// </summary>
        /// <response code="400">Validation failed</response>
        [HttpPost("/ProductCategories")]
        public async Task<ActionResult<ProductCategory>> CreateProductCategory([FromBody]ProductCategory ProductCategory)
        {
            var result = await _productCategoryService.CreateProductCategoryAsync(ProductCategory);
            return Ok(result);
        }


        /// <summary>
        /// Updates a product category.
        /// </summary>
        [HttpPatch("/ProductCategories/{id}")]
        public async Task<ActionResult<bool>> UpdateProductCategory(Guid id, JsonPatchDocument<ProductCategory> patch)
        {
            var ProductCategory = await _productCategoryService.GetProductCategoryAsync(id);

            if (ProductCategory == null)
                return NotFound($"No product category found with ID {id}.");

            patch.ApplyTo(ProductCategory);

            var result = await _productCategoryService.UpdateProductCategoryAsync(ProductCategory);
            return Ok(result);
        }       
    }
}