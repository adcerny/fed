using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using Fed.Web.Portal.Extensions;
using Fed.Web.Portal.Models.Suppliers;
using Fed.Web.Portal.ViewModels;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class SupplierController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;

        public SupplierController(IConfiguration config, FedWebClient fedWebClient)
        {
            _config = config;
            _fedWebClient = fedWebClient;
        }

        //get supplier orders
        [HttpGet("/supplier/{supplierId}")]
        public Task<IActionResult> SupplierOrders2(int supplierId, DateTime deliveryDate)
        {
            return SupplierOrders(supplierId, deliveryDate);
        }

        [HttpGet("/supplier/{supplierId}/{deliveryDate}")]
        public async Task<IActionResult> SupplierOrders(int supplierId, DateTime deliveryDate)
        {
            var date = Date.Create(deliveryDate);

            var supplier = await _fedWebClient.GetSupplierAsync(supplierId);

            var orders = await _fedWebClient.GetSupplierConfirmedOrdersAsync(
                supplierId,
                date);

            if (orders == null || orders.Count == 0)
            {
                var forecast =
                    await _fedWebClient.GetSupplierForecastAsync(
                        supplierId,
                        date);

                orders =
                    forecast.ContainsKey(date)
                    ? forecast[date]
                    : new List<SupplierProductQuantity>();
            }

            var viewModel = new SupplierOrdersViewModel(supplier.Name.ToString(), supplierId, date, orders);
            return View("SupplierOrders", viewModel);
        }

        [HttpGet("/supplier/{supplierId}/forecast")]
        public async Task<IActionResult> ForecastForSupplier(int supplierId)
        {
            var toDate = Date.Today.AddDays(7);
            var supplier = await _fedWebClient.GetSupplierAsync(supplierId);

            var result = await _fedWebClient.GetSupplierForecastAsync(
                supplierId,
                toDate);

            var products = await _fedWebClient.GetProductsAsync();

            var supplierProducts = products.Where(p => p.SupplierId == supplierId.ToString()).ToList();

            var viewModel = new SupplierForecastViewModel(supplier.Name.ToString(), supplierId, toDate, supplierProducts, result);

            return View("SupplierForecast", viewModel);
        }

        [HttpGet("/suppliers/create")]
        public IActionResult CreateSupplier()
        {
            var model = CreateSupplierModel.CreateEmpty();

            return View("CreateSupplier", model);
        }

        [HttpPost("/suppliers/create")]
        public async Task<IActionResult> CreateSupplier(CreateSupplierModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("CreateSupplier", model);

                var supplier = new Supplier(model.SupplierId, model.SupplierName);
                var createdSupplier = await _fedWebClient.CreateSupplierAsync(supplier);
                if (createdSupplier == null)
                {
                    ViewBag.Success = false;
                    return View("CreateSupplier", model);

                }

                ViewBag.Success = true;
                return RedirectToAction("Suppliers", "Dashboard");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Supplier", ex.GetFriendlyErrorMessage());
                return View("CreateSupplier", model);
            }
        }

        [HttpGet("/supplier/overview/{supplierId}")]
        public async Task<IActionResult> GetSupplierOverview(int supplierId)
        {
            var model = await _fedWebClient.GetSupplierAsync(supplierId);

            return View("SupplierOverview", model);
        }

        [HttpGet("/supplier/info/{supplierId}")]
        public async Task<IActionResult> SupplierInfo(int supplierId)
        {
            var supplier = await _fedWebClient.GetSupplierAsync(supplierId);

            if (supplier == null)
                throw new Exception($"Supplier with ID {supplierId} does not exist.");


            return View("SupplierInfo", new SupplierInfoModel(supplier.Id, supplier.Name));
        }

        [HttpPost("/supplier/info/{supplierId}")]
        public async Task<IActionResult> SaveSupplierInfo(CreateSupplierModel model)
        {
            var supplier = await _fedWebClient.GetSupplierAsync(model.SupplierId);


            if (supplier == null)
                throw new Exception($"Supplier with ID {model.SupplierId} does not exist.");

            await _fedWebClient.PatchSupplierAsync(model.SupplierId, PatchOperation.CreateReplace("/name", model.SupplierName));

            return RedirectToAction("GetSupplierOverview", "Supplier", new { supplierId = model.SupplierId });
        }

    }
}