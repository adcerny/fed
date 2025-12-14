using Fed.Core.Enums;
using Fed.Core.ValueTypes;
using Fed.Web.Service.Client;
using Fed.Web.SupplierPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Web.SupplierPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;

        public HomeController(IConfiguration config, FedWebClient fedWebClient)
        {
            _config = config;
            _fedWebClient = fedWebClient;
        }

        [HttpGet("/sevenSeeded/forecast")]
        public async Task<IActionResult> SevenSeededForecast()
        {
            var sevenSeededSupplierId = 7;
            var toDate = Date.Today.AddDays(10);
            var suppliers = await _fedWebClient.GetSuppliersAsync();
            var supplier = suppliers.Where(s => s.Id == sevenSeededSupplierId).Single();

            var result = await _fedWebClient.GetSupplierForecastAsync(
                sevenSeededSupplierId,
                toDate);

            var products = await _fedWebClient.GetProductsAsync();

            var supplierProducts = products.Where(p => p.SupplierId == sevenSeededSupplierId.ToString()).ToList();

            var viewModel = new SupplierForecastViewModel(supplier.Name, toDate, supplierProducts, result);

            return View("SevenSeededForecast", viewModel);
        }

        [HttpGet("/yummytummy/forecast")]
        public async Task<IActionResult> YummyTummyForecast()

        {
            var yummyTummySupplierId = 291;
            var toDate = Date.Today.AddDays(10);

            var suppliers = await _fedWebClient.GetSuppliersAsync();
            var supplier = suppliers.FirstOrDefault(s => s.Id == yummyTummySupplierId);

            if (supplier == null)
                return new NotFoundObjectResult("Supplier not found");

            var result = await _fedWebClient.GetSupplierForecastAsync(
                yummyTummySupplierId,
                toDate);

            var products = await _fedWebClient.GetProductsAsync();

            var supplierProducts = products.Where(p => p.SupplierId == yummyTummySupplierId.ToString()).ToList();

            var viewModel = new SupplierForecastViewModel(supplier.Name, toDate, supplierProducts, result);

            return View("YummyTummyForecast", viewModel);
        }

        [HttpGet("/test")]
        public async Task<IActionResult> Test()
        {
            // Set the forecast date to 3 days ahead
            var today = DateTime.Today;
            var toDate = today.AddDays(3);

            // Pull the forecast
            var forecast = await _fedWebClient.GetSupplierForecastAsync((int)Suppliers.SevenSeeded, toDate);

            // Create a key for the forecast
            var data = new StringBuilder();
            var dates = forecast.Keys;

            foreach (var date in dates)
            {
                data.Append(date.ToString("yyyyMMdd"));

                var productQuantities = forecast[date];

                foreach (var pq in productQuantities)
                {
                    data.Append($"{pq.SupplierSKU}{pq.SupplierQuantity}");
                }
            }

            //Hash the key for faster lookup queries:
            using (var hasher = SHA256.Create())
            {
                var bytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(data.ToString()));
                var hash = BitConverter.ToString(bytes).Replace("-", "").ToLower();
                return Ok(hash);
            }
        }
    }
}