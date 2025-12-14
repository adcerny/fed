using Fed.Core.Entities;
using Fed.Web.Portal.Models.CustomerMarketingAttributes;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class CustomerMarketingAttributesController : Controller
    {
        private readonly FedWebClient _fedWebClient;

        public CustomerMarketingAttributesController(FedWebClient fedWebClient)
        {
            _fedWebClient = fedWebClient;

        }

        [HttpPost("/customermarketingattribute/create")]
        public async Task<JsonResult> CreateCustomerMarketingAttribute([FromBody]CreateCustomerMarketingAttributeModel model)
        {
            var marketingAttribute = new CustomerMarketingAttribute(Guid.Empty, model.Name, model.Description);
            var createdMarketingAttribute = await _fedWebClient.CreateCustomerMarketingAttributeAsync(marketingAttribute);
            return Json(createdMarketingAttribute);
        }

        [HttpGet("/customermarketingattribute/{Id}")]
        public async Task<IActionResult> CustomerMarketingAttributeInfo(string Id)
        {
            var marketingAttribute = await _fedWebClient.GetCustomerMarketingAttributeAsync(Guid.Parse(Id));
            if (marketingAttribute == null)
                throw new Exception($"Marketing attribute with ID {Id} does not exist.");
            return Json(marketingAttribute);
        }

        [HttpGet("/customermarketingattributes")]
        public async Task<JsonResult> GetCustomerMarketingAttributes()
        {
            var customerMarketingAttributes = await _fedWebClient.GetCustomerMarketingAttributesAsync();
            return Json(customerMarketingAttributes);
        }


        [HttpPost("/customermarketingattribute/update")]
        public async Task<JsonResult> SaveCustomerMarketingAttributeInfo([FromBody]CustomerMarketingAttributeInfoModel model)
        {
            var createdMarketingAttribute = await _fedWebClient.GetCustomerMarketingAttributeAsync(model.Id);

            if (createdMarketingAttribute == null)
                throw new Exception($"Marketing attribute with ID {createdMarketingAttribute} does not exist.");

            await _fedWebClient.PatchCustomerMarketingAttributeAsync(createdMarketingAttribute.Id, PatchOperation.CreateReplace("/name", model.Name), PatchOperation.CreateReplace("/description", model.Description));

            return Json("ok");
        }
    }
}