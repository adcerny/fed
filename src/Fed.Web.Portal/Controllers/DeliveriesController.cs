using Fed.Api.External.AzureStorage;
using Fed.Core.Entities;
using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using Fed.Label.V1;
using Fed.Label.V2;
using Fed.Web.Portal.Extensions;
using Fed.Web.Portal.Models.Deliveries;
using Fed.Web.Portal.ViewModels;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class DeliveriesController : Controller
    {
        private readonly FedWebClient _fedWebClient;
        private readonly IAzureTableService _tableService;
        private readonly IPrintLabelService _printLabelService;
        private readonly IConfiguration _config;

        public DeliveriesController(
            IConfiguration config,
            FedWebClient fedWebClient,
            IAzureTableService tableService,
            IPrintLabelService printLabelService)
        {
            _config = config;
            _fedWebClient = fedWebClient;
            _tableService = tableService;
            _printLabelService = printLabelService;
        }

        private IEnumerable<Delivery> SortOrders(IList<Delivery> deliveries, string sortBy, bool asc)
        {
            if (asc)
            {
                switch (sortBy)
                {
                    case "Day": return deliveries.OrderBy(o => o.DeliveryDate.Value.DayOfWeek);
                    case "Date": return deliveries.OrderBy(o => o.DeliveryDate.Value);
                    case "Slot": return deliveries.OrderBy(o => o.EarliestTime).ThenBy(o => o.SortIndex).ThenBy(o => o.DeliveryCompanyName);
                    case "DeliveryId": return deliveries.OrderBy(o => o.SortIndex).ThenBy(o => o.DeliveryCompanyName);
                    case "CompanyName": return deliveries.OrderBy(o => o.DeliveryCompanyName);
                    case "ContactName": return deliveries.OrderBy(o => o.DeliveryFullName);
                    case "Postcode": return deliveries.OrderBy(o => o.DeliveryPostcode);
                    case "ItemCount": return deliveries.OrderBy(o => o.GetAggregatedOrderItems().Count);
                    case "FirstDelivery": return deliveries.OrderBy(o => o.IsFirstDelivery());
                    case "PackingStatus": return deliveries.OrderBy(o => o.PackingStatusId);
                    default: return deliveries.OrderBy(o => o.SortIndex).ThenBy(o => o.DeliveryCompanyName);
                }
            }

            switch (sortBy)
            {
                case "Day": return deliveries.OrderByDescending(o => o.DeliveryDate.Value.DayOfWeek);
                case "Date": return deliveries.OrderByDescending(o => o.DeliveryDate.Value);
                case "Slot": return deliveries.OrderByDescending(o => o.EarliestTime).ThenByDescending(o => o.SortIndex).ThenByDescending(o => o.DeliveryCompanyName);
                case "DeliveryId": return deliveries.OrderByDescending(o => o.SortIndex).ThenByDescending(o => o.DeliveryCompanyName);
                case "CompanyName": return deliveries.OrderByDescending(o => o.DeliveryCompanyName);
                case "ContactName": return deliveries.OrderByDescending(o => o.DeliveryFullName);
                case "Postcode": return deliveries.OrderByDescending(o => o.DeliveryPostcode);
                case "ItemCount": return deliveries.OrderByDescending(o => o.GetAggregatedOrderItems().Count);
                case "FirstDelivery": return deliveries.OrderByDescending(o => o.IsFirstDelivery());
                case "PackingStatus": return deliveries.OrderByDescending(o => o.PackingStatusId);
                default: return deliveries.OrderByDescending(o => o.SortIndex).ThenByDescending(o => o.DeliveryCompanyName);
            }
        }

        private async Task<IList<Delivery>> LoadDeliveriesAsync(DateTime? deliveryDate = null)
        {
            var dateTime = deliveryDate ?? DateTime.Today;
            var date = Date.Create(dateTime);
            var deliveries = await _fedWebClient.GetDeliveriesAsync(date);

            return deliveries;
        }

        private async Task<DeliveriesViewModel> LoadDeliveryViewModelAsync(DateTime? deliveryDate = null, string sortBy = null, bool asc = true)
        {
            var sortByCol = sortBy ?? "Slot";
            var deliveries = await LoadDeliveriesAsync(deliveryDate);
            var sortedDeliveries = SortOrders(deliveries, sortByCol, asc).ToList();
            var dateTime = deliveryDate ?? DateTime.Today;
            var date = Date.Create(dateTime);

            return
                new DeliveriesViewModel(date)
                {
                    Deliveries = sortedDeliveries,
                    SortBy = sortByCol,
                    Asc = asc
                };
        }
        [Route("/deliveries/{deliveryId}")]
        public async Task<IActionResult> GetDeliveryById(string deliveryId)
        {
            var delivery = await _fedWebClient.GetDeliveryAsync(deliveryId);

            if (delivery == null)
                return NotFound();

            var products = await _fedWebClient.GetProductsAsync();

            var deliveries = await LoadDeliveriesAsync(delivery.DeliveryDate);
            var pickOrder = await _tableService.GetCurrentPickOrderAsync();
            var sortedDeliveries = deliveries.SortDeliveries(pickOrder);

            var prevDeliveryId = "";
            var nextDeliveryId = "";

            for (var i = 0; i < sortedDeliveries.Count; i++)
            {
                var del = sortedDeliveries.ElementAt(i);

                if (del.Id.Equals(delivery.Id))
                {
                    if (i >= 1)
                        prevDeliveryId = sortedDeliveries.ElementAt(i - 1).ShortId;

                    if (i < sortedDeliveries.Count - 1)
                        nextDeliveryId = sortedDeliveries.ElementAt(i + 1).ShortId;

                    break;
                }
            }

            var viewModel = new DeliveryViewModel(
                delivery, products, nextDeliveryId, prevDeliveryId, _config["FED_LABEL_HOST_URL"]);

            return View("Delivery", viewModel);
        }

        [Route("/deliveries")]
        public async Task<IActionResult> Deliveries(DateTime? deliveryDate = null, string sortBy = null, bool asc = true)
        {
            var viewModel = await LoadDeliveryViewModelAsync(deliveryDate, sortBy, asc);

            return View("Deliveries", viewModel);
        }

        [Route("/deliveries/pickSheets")]
        public async Task<IActionResult> PickSheets(DateTime? deliveryDate = null)
        {
            var deliveries = await LoadDeliveriesAsync(deliveryDate);
            var pickOrder = await _tableService.GetCurrentPickOrderAsync();
            var sortedDeliveries = deliveries.SortDeliveries(pickOrder);

            var productCodes = await _tableService.GetCurrentProductOrderAsync() ?? new List<String>();

            var vm = new PicksheetViewModel
            {
                Deliveries = sortedDeliveries,
                ProductOrder = productCodes
            };

            return View("PickSheets", vm);
        }

        [Route("/deliveries/labels")]
        public async Task<IActionResult> GenerateLabels([FromQuery]DateTime? deliveryDate = null)
        {
            var viewModel = await LoadDeliveryViewModelAsync(deliveryDate, null);

            var pdfStream = BagLabel.GenerateBagLabels(viewModel.Deliveries, 7);

            return File(pdfStream, "application/pdf", $"Labels-{deliveryDate?.ToString("yyyy-MM-dd")}-{DateTime.Now.ToString("HHmmss")}.pdf");
        }

        [Route("/deliveries/labels/{deliveryId}")]
        public async Task<IActionResult> GenerateLabels([FromRoute]string deliveryId)
        {
            var delivery = await _fedWebClient.GetDeliveryAsync(deliveryId);

            var pdfStream = BagLabel.GenerateBagLabels(new List<Delivery> { delivery }, 21, false);

            return File(pdfStream, "application/pdf", $"Labels-{delivery.ShortId}.pdf");
        }

        [HttpPost("/deliveries/labels/{deliveryId}")]
        public async Task<IActionResult> PrintLabels([FromRoute]string deliveryId, [FromBody] int bagCount)
        {
            var delivery = await _fedWebClient.SetDeliveryBagCountAsync(deliveryId, bagCount);

            _printLabelService.PrintLabels(delivery.ShortId, delivery.DeliveryCompanyName, delivery.DeliveryDate, bagCount);

            return Ok("Success");
        }

        [HttpPost("/deliveries/labels/{deliveryId}/images")]
        public async Task<IActionResult> GetLabels([FromRoute]string deliveryId, [FromBody] int bagCount)
        {
            var delivery = await _fedWebClient.SetDeliveryBagCountAsync(deliveryId, bagCount);
            var images = _printLabelService.GetLabels(delivery.ShortId, delivery.DeliveryCompanyName, delivery.DeliveryDate, bagCount);

            return Json(images);
        }

        [HttpPost("/deliveryAdditions/{deliveryId}")]
        public async Task<IActionResult> AddSubstituteToDelivery(string deliveryId, DeliveryAdditionModel model)
        {
            var delivery = await _fedWebClient.GetDeliveryAsync(deliveryId);

            if (delivery == null)
                return NotFound();

            try
            {
                var products = await _fedWebClient.GetProductsAsync();

                var product = products.SingleOrDefault(p => p.Id.Equals(model.ProductId));

                if (product == null)
                    throw new ArgumentOutOfRangeException("The selected product does not exist in the Fed database anymore.");

                var order = delivery.Orders.First();

                // Create addition for this particular order
                var substitute =
                    DeliveryAddition.CreateSubstitute(
                        order.Id,
                        model.ProductId,
                        product.ProductName,
                        product.ProductCode,
                        product.ActualPrice,
                        product.IsTaxable,
                        model.Quantity,
                        model.Notes,
                        model.DeliveryShortageId);

                await _fedWebClient.CreateDeliveryAddition(deliveryId, substitute);

                return Redirect($"/deliveries/{delivery.ShortId}#deliveryAdditions");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DeliveryShortage", ex.GetFriendlyErrorMessage());

                return Ok(ex.Message);
            }
        }

        [HttpPost("/deliveryAdditions/delete/{deliveryShortId}/{deliveryAdditionId}")]
        public async Task<IActionResult> DeleteDeliveryAddition(string deliveryShortId, Guid deliveryAdditionId)
        {
            await _fedWebClient.DeleteDeliveryAdditionAsync(deliveryAdditionId);

            return Redirect($"/deliveries/{deliveryShortId}#deliveryAdditions");
        }

        [HttpPost("/deliveryShortages/{deliveryId}")]
        public async Task<IActionResult> ShortDeliveryItem(string deliveryId, DeliveryShortageModel model)
        {
            var delivery = await _fedWebClient.GetDeliveryAsync(deliveryId);

            if (delivery == null)
                return NotFound();

            try
            {
                switch (model.ReasonCode)
                {
                    case "LIU": model.Reason = "Supplier Shortage (Unavailable)"; break;
                    case "LIA": model.Reason = "Supplier Shortage (Missing Item)"; break;
                    case "BFV": model.Reason = "Supplier Quality"; break;
                    case "LID": model.Reason = "Damage in Transit"; break;
                    case "OED": model.Reason = "Warehouse Damage"; break;
                    case "OER": model.Reason = "Warehouse Error"; break;
                    case "LOU": model.Reason = "Delivery Error"; break;
                    default: break;
                }

                var remainingShortageAmount = model.DesiredQuantity - model.ActualQuantity;

                foreach (var order in delivery.Orders)
                {
                    // Find the order item which needs to be shorted
                    var orderItem = order.OrderItems?.SingleOrDefault(i => i.ProductId.Equals(model.ProductId, StringComparison.OrdinalIgnoreCase) &&
                                                                           i.ActualPrice == model.ProductPrice);

                    if (orderItem == null)
                        continue;

                    // Calculate actual quantity and update remainingShortageAmount
                    var remainder = orderItem.Quantity - remainingShortageAmount;

                    var actualQuantity = 0;

                    if (remainder > 0)
                    {
                        actualQuantity = remainder;
                        remainingShortageAmount = 0;
                    }
                    else if (remainder <= 0)
                    {
                        remainingShortageAmount -= orderItem.Quantity;
                    }

                    // Create shortage for this particular order
                    var deliveryShortage =
                        new DeliveryShortage(
                            Guid.Empty,
                            order.Id,
                            model.ProductId,
                            orderItem.Quantity,
                            actualQuantity,
                            model.Reason,
                            model.ReasonCode,
                            DateTime.Now.TimeOfDay,
                            orderItem.ActualPrice);

                    await _fedWebClient.ShortDeliveryItemAsync(deliveryShortage);

                    // Stop shorting if there is nothing to short anymore
                    if (remainingShortageAmount <= 0)
                        break;
                }

                return Redirect($"/deliveries/{delivery.ShortId}#deliveryShortages");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("DeliveryShortage", ex.GetFriendlyErrorMessage());

                return Ok(ex.Message);
            }
        }

        [HttpPost("/deliveryShortages/delete/{deliveryShortId}/{deliveryShortageId}")]
        public async Task<IActionResult> DeleteDeliveryShortage(string deliveryShortId, Guid deliveryShortageId)
        {
            await _fedWebClient.DeleteDeliveryShortageAsync(deliveryShortageId);

            return Redirect($"/deliveries/{deliveryShortId}#deliveryShortages");
        }

        [HttpPost("/deliveries/{deliveryId}/packingStatus")]
        public async Task<IActionResult> SetPackingStatus([FromRoute]Guid deliveryId, int packingStatusId)
        {
            var delivery = await _fedWebClient.SetDeliveryPackagingStatusAsync(deliveryId, packingStatusId);

            return Redirect($"/deliveries/{delivery.ShortId}");
        }

        [HttpPost("/deliveries/{deliveryId}/bagCount")]
        public async Task<IActionResult> SetBagCount([FromRoute]string deliveryId, int bagCount)
        {
            var delivery = await _fedWebClient.SetDeliveryBagCountAsync(deliveryId, bagCount);

            return Redirect($"/deliveries/{delivery.ShortId}");
        }

        [HttpGet("/deliveries/setPickOrder")]
        public async Task<IActionResult> SetPickOrder()
        {
            var pickOrder = await _tableService.GetCurrentPickOrderAsync() ?? new DeliveryPickOrder();

            return View("SetPickOrder", pickOrder);
        }

        [HttpPost("/deliveries/setPickOrder")]
        public async Task<IActionResult> SetPickOrder(DeliveryPickOrder pickOrder)
        {
            await _tableService.SetPickOrderAsync(pickOrder);

            ViewBag.Success = true;
            return View("SetPickOrder", pickOrder);
        }

        [HttpGet("/deliveries/setProductOrder")]
        public async Task<IActionResult> SetProductOrder()
        {
            var productCodes = await _tableService.GetCurrentProductOrderAsync() ?? new List<String>();

            var products = await _fedWebClient.GetProductsAsync();

            var orderedProducts = products.OrderBySequence(productCodes, p => p.ProductCode).ToList();

            return View("SetProductOrder", orderedProducts);
        }

        [HttpPost("/deliveries/setProductOrder")]
        public async Task<IActionResult> SetProductOrder(List<String> productCodes)
        {
            await _tableService.SetProductOrderAsync(productCodes);

            var products = await _fedWebClient.GetProductsAsync();

            return new OkResult();
        }
    }
}