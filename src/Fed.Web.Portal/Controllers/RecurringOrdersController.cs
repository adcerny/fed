using Fed.Core.Entities;
using Fed.Core.ValueTypes;
using Fed.Web.Portal.ViewModels;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class RecurringOrdersController : Controller
    {
        private readonly FedWebClient _fedWebClient;

        public RecurringOrdersController(FedWebClient fedWebClient)
        {
            _fedWebClient = fedWebClient;
        }

        [HttpGet("/recurringOrders")]
        public async Task<IActionResult> RecurringOrders()
        {
            var hubs = await _fedWebClient.GetHubsAsync();
            var timeslots = await _fedWebClient.GetTimeslotsAsync(hubs[0].Id);
            var products = await _fedWebClient.GetProductsAsync();
            var customers = await _fedWebClient.GetCustomersAsync(true);
            var recurringOrders = await _fedWebClient.GetRecurringOrdersAsync(Date.Today.AddDays(1), Date.MaxDate);

            var viewModel = new RecurringOrdersViewModel(timeslots, customers, products, recurringOrders, Date.MinDate);

            return View("RecurringOrders", viewModel);
        }

        [HttpGet("/recurringOrders/{recurringOrderId}")]
        public async Task<IActionResult> RecurringOrders(Guid recurringOrderId)
        {
            var hubs = await _fedWebClient.GetHubsAsync();
            var timeslots = await _fedWebClient.GetTimeslotsAsync(hubs[0].Id);
            var products = await _fedWebClient.GetProductsAsync();
            var customers = await _fedWebClient.GetCustomersAsync(true);
            var recurringOrder = await _fedWebClient.GetRecurringOrderAsync(recurringOrderId);
            var skipDates = await _fedWebClient.GetSkipDatesAsync(recurringOrderId);

            Contact contact = null;
            Customer customer = null;

            foreach (var cu in customers)
            {
                foreach (var ct in cu.Contacts)
                {
                    if (ct.Id.Equals(recurringOrder.ContactId))
                    {
                        contact = ct;
                        customer = cu;
                        break;
                    }
                }

                if (contact != null)
                    break;
            }

            var viewModel =
                new SingleRecurringOrderViewModel(
                    timeslots,
                    products,
                    skipDates,
                    customer,
                    contact,
                    recurringOrder);

            return View("SingleRecurringOrder", viewModel);
        }

        [HttpDelete("/recurringOrders/{recurringOrderId}")]
        public async Task<IActionResult> DeleteRecurringOrder(Guid recurringOrderId)
        {
            bool result = await _fedWebClient.DeleteRecurringOrderAsync(recurringOrderId);
            if (!result)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("/recurringOrders/{recurringOrderId}/Free")]
        public async Task<IActionResult> RecurringOrderFree([FromRoute]Guid recurringOrderId, [FromQuery]bool isFree)
        {
            var patchOperation = PatchOperation.CreateReplace("/isFree", isFree);
            await _fedWebClient.PatchRecurringOrderAsync(recurringOrderId, patchOperation);
            return Redirect($"/recurringOrders/{recurringOrderId}#charges");
        }

        [HttpPost("/recurringOrders/SkipDeliveryDates")]
        public async Task<IActionResult> RecurringOrderSkipDeliveryDates(SkipDeliveryDatesViewModel viewModel)
        {
            var dates = await _fedWebClient.SetSkipDateAsync(viewModel.OrderId, viewModel.SkipDate, viewModel.Reason, viewModel.CreatedBy);
            if (dates != null)
            {
                ViewData["success"] = "1";
                if (viewModel.RedirectTo == "recurringOrders")
                {
                    return Redirect($"/recurringOrders/{viewModel.OrderId}");
                }

                return Redirect($"/customers/{viewModel.CustomerId}#upcomingOrders");
            }

            ViewData["success"] = "0";
            if (viewModel.RedirectTo == "recurringOrders")
            {
                return Redirect($"/recurringOrders/{viewModel.OrderId}");
            }
            return this.Redirect($"/customers/{viewModel.CustomerId}#upcomingOrders");
        }

        [HttpGet("/recurringOrders/GetSkipDeliveryDates")]
        public async Task<IActionResult> RecurringOrderGetSkipDeliveryDates(Guid orderId, string customerId)
        {

            var dates = await _fedWebClient.GetSkipDatesAsync(orderId);
            if (dates != null)
            {
                ViewData["success"] = "1";
                return this.PartialView("~/Views/Partial/_SkipDeliveryDates.cshtml");
            }

            ViewData["success"] = "0";
            return this.PartialView("~/Views/Partial/_SkipDeliveryDates.cshtml");
        }

        [HttpPost("/recurringOrders/DeleteSkipDeliveryDates/{orderId}/{date}")]
        public async Task<IActionResult> RecurringOrderDeleteSkipDeliveryDates(Guid orderId, string date)
        {

            var dates = await _fedWebClient.DeleteSkipDateAsync(orderId, DateTime.Parse(date));
            if (dates != null)
            {
                ViewData["success"] = "1";
                return Redirect($"/recurringOrders/{orderId}");
            }

            ViewData["success"] = "0";
            return Redirect($"/recurringOrders/{orderId}");
        }
    }
}