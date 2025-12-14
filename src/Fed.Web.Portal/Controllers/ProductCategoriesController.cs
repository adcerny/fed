using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fed.Core.Entities;
using Fed.Web.Portal.Extensions;
using Fed.Web.Portal.Models.CustomerMarketingAttributes;
using Fed.Web.Portal.Models.ProductCategories;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;

namespace Fed.Web.Portal.Controllers
{
    public class ProductCategoriesController : Controller
    {
        private readonly FedWebClient _fedWebClient;

        public ProductCategoriesController(FedWebClient fedWebClient)
        {
            _fedWebClient = fedWebClient;

        }

        [HttpPost("/productcategory/create")]
        public async Task<JsonResult> CreateProductCategory([FromBody]CreateProductCategoryModel model)
        {
                var productCategory = new ProductCategory(Guid.Empty,model.Name);
                var createdProductCategory = await _fedWebClient.CreateProductCategoryAsync(productCategory);
                return Json(createdProductCategory);           
        }        

        [HttpGet("/productcategory/{Id}")]
        public async Task<IActionResult> ProductCategoryInfo(string Id)
        {
            var productCategory = await _fedWebClient.GetProductCategoryAsync(Guid.Parse(Id));
            if (productCategory == null)
                throw new Exception($"Product category with ID {Id} does not exist.");
            return Json(productCategory);
        }

        [HttpGet("/productcategories")]
        public async Task<JsonResult> GetProductCategories()
        {
            var productCategories = await _fedWebClient.GetProductCategoriesAsync();
            return Json(productCategories);
        }


        [HttpPost("/productcategory/update")]
        public async Task<JsonResult> SaveProductCategoryInfo([FromBody]ProductCategoryInfoModel model)
        {
            var productCategory = await _fedWebClient.GetProductCategoryAsync(model.Id);

            if (productCategory == null)
                throw new Exception($"Product category with ID {productCategory} does not exist.");

            await _fedWebClient.PatchProductCategoryAsync(productCategory.Id, PatchOperation.CreateReplace("/name", model.Name));

            return Json("ok");
        }
    }
}