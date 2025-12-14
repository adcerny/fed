using Fed.Api.External.ActivityLogs;
using Fed.Api.External.MerchelloService;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.ValueTypes;
using Fed.Web.Portal.Extensions;
using Fed.Web.Portal.Models.Customers;
using Fed.Web.Portal.ViewModels;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class CustomersController : Controller
    {
        private readonly FedWebClient _fedWebClient;
        private readonly MerchelloAPIClient _merchelloClient;
        private readonly LogonActivityClient _logonActivityClient;
        private readonly ICRMService _crmService;

        public CustomersController(
            FedWebClient fedWebClient,
            MerchelloAPIClient merchelloClient,
            LogonActivityClient logonActivityClient,
            ICRMService crmService)
        {
            _fedWebClient = fedWebClient;
            _merchelloClient = merchelloClient;
            _logonActivityClient = logonActivityClient;
            _crmService = crmService;
        }

        // Private Helper Methods

        private async Task<IActionResult> DisplayCustomerAsync(string customerId)
        {
            var customer = await _fedWebClient.GetCustomerFullInfoAsync(customerId);

            if (customer == null)
                return NotFound($"No customer found with the given ID.");

            var upcomingOrders =
                await _fedWebClient.GetForecastAsync(
                    Date.Today,
                    Date.Today.AddWeeks(8),
                    customer.PrimaryContact.Id);


            var products = await _fedWebClient.GetProductsAsync();

            var logonHistory = await _logonActivityClient.GetLogonHistoryAsync(customer.PrimaryContact.Email);

            var discounts = await _fedWebClient.GetDiscountsForCustomerAsync(customer.Id);

            var viewModel = new CustomerViewModel
            {
                Customer = customer,
                UpcomingOrders = upcomingOrders,
                Products = products,
                LogonHistory = logonHistory,
                Discounts = discounts
            };

            return View("View", viewModel);
        }

        private async Task<IActionResult> FindCustomerAsync(string searchQuery)
        {
            var customerId = Guid.Empty;

            // Get all customers
            var customers = await _fedWebClient.GetCustomersAsync(true);

            // Try find by Customer ID
            var result = customers.Where(c => c.Id.ToString().Contains(searchQuery, StringComparison.OrdinalIgnoreCase));

            if (result != null && result.Count() > 0)
                return await DisplayCustomerAsync(result.First().Id.ToString());

            // Try find by Customer Short ID
            result = customers.Where(c => c.ShortId.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));

            if (result != null && result.Count() > 0)
                return await DisplayCustomerAsync(result.First().Id.ToString());

            // Try find by Customer's Company Name
            result = customers.Where(c => c.CompanyName.Contains(searchQuery, StringComparison.OrdinalIgnoreCase));

            if (result != null && result.Count() > 0)
                return await DisplayCustomerAsync(result.First().Id.ToString());

            return NotFound($"No customer could be found which matches the search query '{searchQuery}'.");
        }

        private async Task SaveStep1CustomerDetails(CreateCustomerModel model)
        {
            if (model.IsExistingCustomer)
            {
                var customer = await _fedWebClient.GetCustomerAsync(model.CustomerId);

                if (customer == null)
                    throw new Exception($"Customer with ID {model.CustomerId} does not exist.");

                model.PatchCustomer(customer);

                var contact = customer.Contacts.First();
                var deliveryAddress = contact.GetPrimaryDeliveryAddress();

                if (model.HasMandatoryDeliveryDetails)
                    await _fedWebClient.PatchDeliveryAddressAsync(
                        customer.Id,
                        contact.Id,
                        deliveryAddress.Id,
                        PatchOperation.CreateReplace("/fullName", deliveryAddress.FullName),
                        PatchOperation.CreateReplace("/companyName", deliveryAddress.CompanyName),
                        PatchOperation.CreateReplace("/addressLine1", deliveryAddress.AddressLine1),
                        PatchOperation.CreateReplace("/addressLine2", deliveryAddress.AddressLine2),
                        PatchOperation.CreateReplace("/postcode", deliveryAddress.Postcode),
                        PatchOperation.CreateReplace("/town", deliveryAddress.Town),
                        PatchOperation.CreateReplace("/deliveryInstructions", deliveryAddress.DeliveryInstructions),
                        PatchOperation.CreateReplace("/phone", deliveryAddress.Phone));

                await _fedWebClient.PatchContactAsync(
                    customer.Id,
                    contact.Id,
                    PatchOperation.CreateReplace("/firstName", contact.FirstName),
                    PatchOperation.CreateReplace("/lastName", contact.LastName),
                    PatchOperation.CreateReplace("/email", contact.Email),
                    PatchOperation.CreateReplace("/phone", contact.Phone),
                    PatchOperation.CreateReplace("/isMarketingConsented", contact.IsMarketingConsented));

                await _fedWebClient.PatchCustomerAsync(
                    customer.Id,
                    PatchOperation.CreateReplace("/companyName", customer.CompanyName),
                    PatchOperation.CreateReplace("/website", model.Website),
                    PatchOperation.CreateReplace("/notes", model.Notes),
                    PatchOperation.CreateReplace("/aCAccountNumber", model.ACAccountNumber),
                    PatchOperation.CreateReplace("/source", model.Source),
                    PatchOperation.CreateReplace("/officeSizeMin", customer.OfficeSizeMin),
                    PatchOperation.CreateReplace("/officeSizeMax", customer.OfficeSizeMax),
                    PatchOperation.CreateReplace("/accountType", model.AccountTypeId));
            }
            else
            {
                var hubs = await _fedWebClient.GetHubsAsync();
                var hubId = hubs.First().Id;
                var prospect = new Customer(
                    Guid.Empty,     // Customer ID
                    string.Empty,   // Customer Short ID
                    "",
                    "",
                    null,           // AC Account Number
                    false,
                    null,
                    null,
                    false,
                    false,
                    false,
                    AccountType.Standard,
                    "",
                    "",
                    false,
                    null);          // Cancellation Reason
                model.PatchCustomer(prospect, hubId);

                // Don't save any billing details at this point yet
                prospect.Contacts.First().BillingAddresses = null;

                if (!model.HasMandatoryDeliveryDetails)
                    prospect.Contacts.First().DeliveryAddresses = null;

                var createdCustomer = await _fedWebClient.CreateCustomerAsync(prospect);
                model.CustomerId = createdCustomer.Id;


                var contact = createdCustomer.Contacts.Single();
                model.ContactId = contact.Id;

                await _merchelloClient.CreateUserAsync(
                new User
                {
                    EmailAddress = contact.Email,
                    CompanyName = createdCustomer.CompanyName,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    CustomerId = createdCustomer.Id.ToString(),
                    ContactId = contact.Id.ToString(),
                    Password = "5yjCPe8c33eYQb2v"
                });

                if (model.SendPasswordResetEmail)
                    await _merchelloClient.ForcePasswordResetEmail(contact.Email);

                model.MerchelloCustomerCreated = true;

            }
        }

        private async Task<Customer> SaveStep2CustomerDetails(CreateCustomerModel model)
        {
            var customer = await _fedWebClient.GetCustomerAsync(model.CustomerId);

            if (customer == null)
                throw new Exception($"Customer with ID {model.CustomerId} does not exist.");

            model.PatchCustomer(customer);

            var contact = customer.Contacts.First();
            var billingAddress = contact.BillingAddresses.Single(a => a.IsPrimary);

            if (billingAddress.Id == Guid.Empty)
            {
                await _fedWebClient.CreateBillingAddressAsync(
                    customer.Id,
                    contact.Id,
                    billingAddress);
            }
            else
            {
                await _fedWebClient.PatchBillingAddressAsync(
                    customer.Id,
                    contact.Id,
                    billingAddress.Id,
                    PatchOperation.CreateReplace("/fullName", billingAddress.FullName),
                    PatchOperation.CreateReplace("/companyName", billingAddress.CompanyName),
                    PatchOperation.CreateReplace("/email", billingAddress.Email),
                    PatchOperation.CreateReplace("/phone", billingAddress.Phone),
                    PatchOperation.CreateReplace("/addressLine1", billingAddress.AddressLine1),
                    PatchOperation.CreateReplace("/addressLine2", billingAddress.AddressLine2),
                    PatchOperation.CreateReplace("/postcode", billingAddress.Postcode),
                    PatchOperation.CreateReplace("/town", billingAddress.Town),
                    PatchOperation.CreateReplace("/invoiceReference", billingAddress.InvoiceReference));
            }

            await _fedWebClient.PatchContactAsync(
                customer.Id,
                contact.Id,
                PatchOperation.CreateReplace("/paymentMethod", model.PaymentMethod));

            await _fedWebClient.PatchCustomerAsync(
                customer.Id,
                PatchOperation.CreateReplace("/isInvoiceable", !model.ExcludeFromInvoicing),
                PatchOperation.CreateReplace("/isDeliveryChargeExempt", model.IsDeliveryChargeExempt),
                PatchOperation.CreateReplace("/splitDeliveriesByOrder", model.SplitDeliveriesByOrder),
                PatchOperation.CreateReplace("/isTestAccount", model.IsTestAccount),
                PatchOperation.CreateReplace("/isFriend", model.IsFriend));

            return customer;
        }

        // View/Find Customers

        [HttpGet("/customers/{customerId}")]
        public Task<IActionResult> DisplayCustomer(string customerId)
        {
            return DisplayCustomerAsync(customerId);
        }

        [HttpGet("/customers")]
        public Task<IActionResult> FindCustomer(string q)
        {
            return FindCustomerAsync(q);
        }

        // Update Customer

        [HttpGet("/customers/{customerId}/accountInfo")]
        public async Task<IActionResult> DisplayAccountInfo(string customerId)
        {
            var customer = await _fedWebClient.GetCustomerFullInfoAsync(customerId);

            if (customer == null)
                return NotFound($"No customer found with the given ID.");

            var accountInfo = AccountInfoModel.FromCustomer(customer);
            accountInfo.MarketingAttributes = await _fedWebClient.GetCustomerMarketingAttributesAsync();
            return View("AccountInfo", accountInfo);
        }

        [HttpPost("/customers/{customerId}/accountInfo")]
        public async Task<IActionResult> UpdateAccountInfo(AccountInfoModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("AccountInfo", model);

                if (!model.IsValidData(out List<string> errorMessages))
                {
                    for (var i = 0; i < errorMessages.Count; i++)
                        ModelState.AddModelError($"err{i}", errorMessages[i]);

                    return View("AccountInfo", model);
                }

                var customer = await _fedWebClient.GetCustomerFullInfoAsync(model.CustomerId.ToString());

                if (customer == null)
                    return NotFound($"No customer found with the given ID.");

                var contact = customer.PrimaryContact;
                var deliveryAddress = contact.GetPrimaryDeliveryAddress();

                var hasDeliveryAddress = deliveryAddress != null;

                if (!hasDeliveryAddress)
                {
                    var hubs = await _fedWebClient.GetHubsAsync();
                    var hubId = hubs.First().Id;

                    if (contact.DeliveryAddresses == null)
                        contact.DeliveryAddresses = new List<DeliveryAddress>();

                    contact.DeliveryAddresses.Add(DeliveryAddress.CreateEmpty(hubId));

                    deliveryAddress = contact.GetPrimaryDeliveryAddress();
                }

                model.PatchCustomer(customer);

                if (model.HasMandatoryDeliveryDetails)
                {
                    if (!hasDeliveryAddress)
                    {
                        await _fedWebClient.CreateDeliveryAddressAsync(
                            customer.Id,
                            contact.Id,
                            deliveryAddress);
                    }
                    else
                    {
                        await _fedWebClient.PatchDeliveryAddressAsync(
                            customer.Id,
                            contact.Id,
                            deliveryAddress.Id,
                            PatchOperation.CreateReplace("/fullName", deliveryAddress.FullName),
                            PatchOperation.CreateReplace("/companyName", deliveryAddress.CompanyName),
                            PatchOperation.CreateReplace("/addressLine1", deliveryAddress.AddressLine1),
                            PatchOperation.CreateReplace("/addressLine2", deliveryAddress.AddressLine2),
                            PatchOperation.CreateReplace("/postcode", deliveryAddress.Postcode),
                            PatchOperation.CreateReplace("/town", deliveryAddress.Town),
                            PatchOperation.CreateReplace("/deliveryInstructions", deliveryAddress.DeliveryInstructions),
                            PatchOperation.CreateReplace("/phone", deliveryAddress.Phone));
                    }
                }

                await _fedWebClient.PatchContactAsync(
                    customer.Id,
                    contact.Id,
                    PatchOperation.CreateReplace("/firstName", contact.FirstName),
                    PatchOperation.CreateReplace("/lastName", contact.LastName),
                    PatchOperation.CreateReplace("/email", contact.Email),
                    PatchOperation.CreateReplace("/phone", contact.Phone),
                    PatchOperation.CreateReplace("/isMarketingConsented", contact.IsMarketingConsented));

                await _fedWebClient.PatchCustomerAsync(
                    customer.Id,
                    PatchOperation.CreateReplace("/companyName", customer.CompanyName),
                    PatchOperation.CreateReplace("/website", model.Website),
                    PatchOperation.CreateReplace("/notes", model.Notes),
                    PatchOperation.CreateReplace("/aCAccountNumber", model.ACAccountNumber),
                    PatchOperation.CreateReplace("/source", model.Source),
                    PatchOperation.CreateReplace("/officeSizeMin", customer.OfficeSizeMin),
                    PatchOperation.CreateReplace("/officeSizeMax", customer.OfficeSizeMax),
                    PatchOperation.CreateReplace("/accountType", model.AccountTypeId),
                    PatchOperation.CreateReplace("/isInvoiceable", !model.ExcludeFromInvoicing),
                    PatchOperation.CreateReplace("/isDeliveryChargeExempt", model.IsDeliveryChargeExempt),
                    PatchOperation.CreateReplace("/splitDeliveriesByOrder", model.SplitDeliveriesByOrder),
                    PatchOperation.CreateReplace("/isTestAccount", model.IsTestAccount),
                    PatchOperation.CreateReplace("/isFriend", model.IsFriend),
                    PatchOperation.CreateReplace("/cancellationReason", model.CancellationReason));

                if (model.IsDirty == "1")
                {
                    await _merchelloClient.UpdateUserAsync(
               new User
               {
                   EmailAddress = contact.Email,
                   CompanyName = model.CompanyName,
                   FirstName = model.ContactFirstName,
                   LastName = model.ContactLastName,
                   CustomerId = model.CustomerId.ToString(),
                   ContactId = model.ContactId.ToString()
               });
                }

                ViewBag.Success = true;
                return View("AccountInfo", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Customer", ex.GetFriendlyErrorMessage());
                return View("AccountInfo", model);
            }
        }

        [HttpGet("/customers/{customerId}/paymentInfo")]
        public async Task<IActionResult> DisplayPaymentInfo(string customerId)
        {
            var customer = await _fedWebClient.GetCustomerFullInfoAsync(customerId);

            if (customer == null)
                return NotFound($"No customer found with the given ID.");

            var paymentInfo = PaymentInfoModel.FromCustomer(customer);

            return View("PaymentInfo", paymentInfo);
        }

        [HttpPost("/customers/{customerId}/paymentInfo")]
        public async Task<IActionResult> UpdatePaymentInfo(PaymentInfoModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("PaymentInfo", model);

                var customer = await _fedWebClient.GetCustomerFullInfoAsync(model.CustomerId.ToString());

                if (customer == null)
                    return NotFound($"No customer found with the given ID.");

                var contact = customer.PrimaryContact;
                var billingAddress = contact.GetPrimaryBillingAddress();

                var hasBillingAddress = billingAddress != null;

                if (!hasBillingAddress)
                {
                    if (contact.BillingAddresses == null)
                        contact.BillingAddresses = new List<BillingAddress>();

                    contact.BillingAddresses.Add(BillingAddress.CreateEmpty());

                    billingAddress = contact.GetPrimaryBillingAddress();
                }

                model.PatchCustomer(customer);

                if (!hasBillingAddress)
                {
                    await _fedWebClient.CreateBillingAddressAsync(
                        customer.Id,
                        contact.Id,
                        billingAddress);
                }
                else
                {
                    await _fedWebClient.PatchBillingAddressAsync(
                    customer.Id,
                    contact.Id,
                    billingAddress.Id,
                    PatchOperation.CreateReplace("/companyName", billingAddress.CompanyName),
                    PatchOperation.CreateReplace("/addressLine1", billingAddress.AddressLine1),
                    PatchOperation.CreateReplace("/addressLine2", billingAddress.AddressLine2),
                    PatchOperation.CreateReplace("/postcode", billingAddress.Postcode),
                    PatchOperation.CreateReplace("/town", billingAddress.Town),
                    PatchOperation.CreateReplace("/invoiceReference", billingAddress.InvoiceReference),
                    PatchOperation.CreateReplace("/email", billingAddress.Email));
                }

                await _fedWebClient.PatchContactAsync(
                    customer.Id,
                    contact.Id,
                    PatchOperation.CreateReplace("/paymentMethod", model.PaymentMethod));

                ViewBag.Success = true;
                return View("PaymentInfo", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Customer", ex.GetFriendlyErrorMessage());
                return View("PaymentInfo", model);
            }
        }

        // Edit Card Details

        [HttpGet("/customers/{customerId}/paymentInfo/cardDetails")]
        public async Task<IActionResult> Index(Guid customerId)
        {
            try
            {
                var customer = await _fedWebClient.GetCustomerAsync(customerId);

                if (customer == null)
                    return NotFound($"No customer with ID {customerId} could be found.");

                var clientToken = await _fedWebClient.GetClientTokenAsync();

                var model = new CardDetailsModel(customer, clientToken);
                return View("CardDetails", model);
            }
            catch
            {
                return NotFound($"No customer with ID {customerId} could be found.");
            }
        }

        [Route("/customers/{customerId}/paymentInfo/cardDetails")]
        [HttpPost]
        public async Task<IActionResult> CreatePaymentMethod([FromRoute]Guid customerId, [FromRoute]Guid contactId, [FromBody]CardPaymentRequest cardPaymentRequest)
        {
            try
            {
                var customer = await _fedWebClient.GetCustomerAsync(customerId);

                if (customer == null)
                    return NotFound($"No customer with ID {customerId} could be found.");

                var contact = customer.Contacts.FirstOrDefault();

                //Delete any existing card tokens
                foreach (var cardToken in contact.CardTokens.OrEmptyIfNull())
                    await _fedWebClient.DeleteCardTokenAsync(customerId, contact.Id, cardToken.Id);

                var CardToken = await _fedWebClient.PostCardTokenAsync(customerId, contact.Id, cardPaymentRequest);

                return Ok(CardToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Create Customer

        [HttpGet("/customers/create/step1")]
        public async Task<IActionResult> CreateCustomerAsync()
        {
            var model = CreateCustomerModel.CreateEmpty();
            model.MarketingAttributes = await _fedWebClient.GetCustomerMarketingAttributesAsync();

            return View("Create1", model);
        }

        [HttpPost("/customers/create/step1")]
        public async Task<IActionResult> SaveProspect(CreateCustomerModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Create1", model);

                if (!model.IsValidProspect(out List<string> errorMessages))
                {
                    for (var i = 0; i < errorMessages.Count; i++)
                        ModelState.AddModelError($"err{i}", errorMessages[i]);

                    return View("Create1", model);
                }

                await SaveStep1CustomerDetails(model);

                ViewBag.Success = true;
                return View("Create1", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Customer", ex.GetFriendlyErrorMessage());
                return View("Create1", model);
            }
        }

        [HttpPost("/customers/create/step2")]
        public async Task<IActionResult> SaveAndProceedToStep2(CreateCustomerModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Create1", model);

                if (!model.IsValidDeliverableCustomer(out List<string> errorMessages))
                {
                    for (var i = 0; i < errorMessages.Count; i++)
                        ModelState.AddModelError($"err{i}", errorMessages[i]);

                    return View("Create1", model);
                }

                await SaveStep1CustomerDetails(model);

                model.BraintreeClientToken = await _fedWebClient.GetClientTokenAsync();

                ViewBag.Success = true;
                return View("Create2", model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Customer", ex.GetFriendlyErrorMessage());
                return View("Create1", model);
            }
        }

        [HttpPost("/customers/create/finish")]
        public async Task<IActionResult> CreateCustomer(CreateCustomerModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View("Create2", model);

                if (!model.IsValidCustomer(out List<string> errorMessages))
                {
                    for (var i = 0; i < errorMessages.Count; i++)
                        ModelState.AddModelError($"err{i}", errorMessages[i]);

                    return View("Create2", model);
                }

                var createdCustomer = await SaveStep2CustomerDetails(model);
                var contact = createdCustomer.Contacts.First();

                try
                {
                    if (model.PaymentMethod == (int)PaymentMethod.Card)
                    {
                        var cardPaymentRequest =
                            new CardPaymentRequest
                            {
                                Postcode = model.BillingPostCode,
                                AddressLine1 = model.BillingAddressLine1,
                                CardHolderName = model.CardholderName,
                                IsPrimary = true,
                                DeviceData = "",
                                PaymentMethodNonce = model.BraintreeNonce
                            };

                        await _fedWebClient.PostCardTokenAsync(
                            createdCustomer.Id,
                            contact.Id,
                            cardPaymentRequest);
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    throw new Exception($"Card was declined. Message was: {httpEx.Message}", httpEx);
                }

                ViewBag.Success = true;
                return View("Finish", createdCustomer);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("Customer", ex.GetFriendlyErrorMessage());
                return View("Create2", model);
            }
        }

        [HttpGet("/customers/GetCustomerAgents")]
        public async Task<JsonResult> GetCustomerAgents(Guid customerAgentId)
        {
            var agents = await _fedWebClient.GetCustomerAgentsAsync();

            return Json(agents);
        }

        [HttpGet("/customers/UpdateCustomerAgent")]
        public async Task<JsonResult> UpdateCustomerAgent(Guid customerId, Guid? customerAgentId)
        {
            PatchOperation.CreateReplace("/customerAgentId", customerAgentId);

            await _fedWebClient.PatchCustomerAsync(customerId, PatchOperation.CreateReplace("/customerAgentId", customerAgentId));

            return Json(true);
        }

        [HttpGet("/customers/DeleteCustomerAgent")]
        public async Task<JsonResult> DeleteCustomerAgent(Guid customerAgentId)
        {
            await _fedWebClient.DeleteCustomerAgentAsync(customerAgentId);

            return Json(true);
        }

        [HttpPost("/customers/CreateCustomerAgent")]
        public async Task<JsonResult> CreateCustomerAgent([FromBody]CustomerAgent customerAgent)
        {
            var newAgent = await _fedWebClient.CreateCustomerAgentAsync(customerAgent);

            return Json(newAgent);
        }

        [HttpGet("/customers/UpdateCustomerMarketingAttribute")]
        public async Task<JsonResult> UpdateCustomerMarketingAttribute(Guid customerId, Guid? customerMarketingAttributeId)
        {
            await _fedWebClient.PatchCustomerAsync(customerId, PatchOperation.CreateReplace("/customerMarketingAttributeId", customerMarketingAttributeId));

            return Json(true);
        }

        [HttpGet("/customers/AddCrmLead/{customerId}")]
        public async Task<JsonResult> UpdateCustomerMarketingAttribute([FromRoute]Guid customerId)
        {
            var customer = await _fedWebClient.GetCustomerAsync(customerId);
            var result = await _crmService.AddLead(customer);
            return Json(result);
        }
    }
}