using Fed.Core.ValueTypes;
using Fed.Web.Service.Client;
using Fed.Web.SupplierPortal.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.SupplierPortal.Controllers
{
    public class SuppliersController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;

        public SuppliersController(IConfiguration config, FedWebClient fedWebClient)
        {
            _config = config;
            _fedWebClient = fedWebClient;
        }

        [HttpGet("/suppliers/{supplierId}/{supplierName}")]
        public async Task<IActionResult> GetSuppliersForecastAsync([FromRoute]int supplierId, [FromRoute]string supplierName)
        {
            var toDate = Date.Today.AddDays(10);

            var supplier = await _fedWebClient.GetSupplierAsync(supplierId);

            if (supplier == null || supplier.Name.ToLower().Replace(" ", string.Empty) != supplierName.ToLower())
                return new NotFoundObjectResult("Supplier not found");

            var result = await _fedWebClient.GetSupplierForecastAsync(
                supplierId,
                toDate);

            var products = await _fedWebClient.GetProductsAsync();

            var supplierProducts = products.Where(p => p.SupplierId == supplierId.ToString()).ToList();

            var viewModel = new SupplierForecastViewModel(supplier.Name, toDate, supplierProducts, result);

            return View("Forecast", viewModel);
        }
    }
}