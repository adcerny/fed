using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Web.Portal.Models.Discounts;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class DiscountsController : Controller
    {

        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;

        public DiscountsController(IConfiguration config, FedWebClient fedWebClient)
        {
            _config = config;
            _fedWebClient = fedWebClient;
        }

        [HttpGet("/Discounts")]
        public async Task<IActionResult> Index()
        {
            var discounts = await _fedWebClient.GetDiscountsAsync();
            return View("Discounts", discounts);
        }

        [HttpGet("/Discounts/{id}")]
        public async Task<IActionResult> Index([FromRoute] Guid id)
        {

            DiscountModel model = new DiscountModel();

            model.Discount = await _fedWebClient.GetDiscountAsync(id);
            model.Products = await _fedWebClient.GetProductsAsync();
            model.ProductCategories = await _fedWebClient.GetProductCategoriesAsync();


            return View("Discount", model);
        }

        [HttpGet("/discounts/create")]
        public async Task<IActionResult> Create()
        {
            DiscountModel model = new DiscountModel();
            model.Products = await _fedWebClient.GetProductsAsync();
            model.ProductCategories = await _fedWebClient.GetProductCategoriesAsync();

            return View("Discount", model);
        }

        [HttpPost("/discounts/{id}")]
        public async Task<IActionResult> UpdateDiscount([FromRoute]Guid id, [FromForm] DiscountModel model)
        {
            var discount = model.Discount;
            var updatedDiscount = await _fedWebClient.UpdateDiscountAsync(id, discount);

            return RedirectToAction("Index");
        }

        [HttpPost("/discounts/create")]
        public async Task<IActionResult> CreateDiscount([FromForm] DiscountModel model)
        {
            var discount = model.Discount;
            if (discount.EligibleProductsType == 0)
                discount.EligibleProductsType = DiscountEligibleProductsType.AllProducts;

            if (discount.MinOrderValue == 0)
                discount.MinOrderValue = null;

            if (discount.MaxOrderValue == 0)
                discount.MaxOrderValue = null;

            if (discount.Percentage == 0)
                discount.Percentage = null;

            if (discount.Value == 0)
                discount.Value = null;

            if (discount.PeriodFromStartDays == 0)
                discount.PeriodFromStartDays = null;

            var createdDiscount = await _fedWebClient.CreateDiscountAsync(discount);

            return RedirectToAction("Index");
        }

        //[HttpPost("/Discounts/{id}/Code")]
        //public async Task<IActionResult> Discount([FromRoute]Guid id, [FromBody] DiscountCode code)
        //{
        //    var discount = await _fedWebClient.GetDiscountAsync(id);
        //    return View("Discount", discount);
        //}
    }
}