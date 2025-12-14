using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fed.Web.Portal.ViewModels;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;

namespace Fed.Web.Portal.Controllers
{
    public class ProductsController : Controller
    {
        private readonly FedWebClient _fedWebClient;

        public ProductsController(FedWebClient fedWebClient)
        {
            _fedWebClient = fedWebClient;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Builder()
        {
            var productsTask = _fedWebClient.GetProductsAsync();
            var vm = new ProductBuilderViewModel
            {
                Products = await productsTask
            };
            return View(vm);
        }
    }
}