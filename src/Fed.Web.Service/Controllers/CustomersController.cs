using Fed.Api.External.AzureStorage;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Exceptions;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.Services.Validators;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly ICustomerService _customerService;
        

        public CustomersController(
            ICustomerService customerService,
            IPaymentGatewayService paymentGatewayService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
        }

        /// <summary>
        /// Returns a customer matching the supplied Id.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <response code="404">Customer not found.</response>
        [HttpGet("/customers/{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(Guid id)
        {
            try
            {
                var customer = await _customerService.GetCustomer(id);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Returns a customer with their entire delivery history for the supplied Id.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <response code="404">Customer not found.</response>
        [HttpGet("/customers/{id}/fullInfo")]
        public async Task<ActionResult<FullCustomerInfo>> GetFullCustomerInfo(string id)
        {
            try
            {
                var customer = await _customerService.GetFullCustomerInfo(id);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// <summary>
        /// Returns all customers with contacts if specified.
        /// </summary>
        [HttpGet("/customers")]
        public async Task<ActionResult<IList<Customer>>> GetCustomers(bool includeContacts)
        {
            var result = await _customerService.GetCustomers(includeContacts);

            return Ok(result);
        }

        /// <summary>
        /// Creates a new customer.
        /// </summary>
        /// <response code="400">Validation failed</response>
        [HttpPost("/customers")]
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            try
            {
                return await _customerService.CreateCustomer(customer);
            }
            catch (FluentValidationException ex)
            {
                ex.ValidationResult.AddToModelState(ModelState, null);
                return new BadRequestObjectResult(ModelState);
            }
        }

        /// <summary>
        /// Updates an existing customer.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="patch">The patch operation.</param>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Customer not found</response>
        [HttpPatch("/customers/{id}")]
        public async Task<ActionResult<bool>> PatchCustomer(Guid id, JsonPatchDocument<Customer> patch)
        {
            try
            {
                var result = await _customerService.PatchCustomer(id, patch);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (FluentValidationException ex)
            {
                ex.ValidationResult.AddToModelState(ModelState, null);
                return new BadRequestObjectResult(ModelState);
            }   
         }

        /// <summary>
        /// Returns a contact matching the supplied Id.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <response code="404">Contact not found</response>
        [HttpGet("/customers/{id}/contacts/{contactId}")]
        public async Task<ActionResult<Contact>> GetContact(Guid id, Guid contactId)
        {
            var contact = await _customerService.GetContact(id, contactId);

            if (contact == null)
                return NotFound();

            return Ok(contact);
        }

        /// <summary>
        /// Creates a new contact.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contact">The contact to create.</param> 
        /// <response code="200">Contact successfully created</response>
        /// <response code="400">Validation failed</response>
        [HttpPost("/customers/{id}/contacts")]
        public async Task<ActionResult<Guid>> CreateContact(Guid id, Contact contact)
        {
            try
            {
                var createdContact = await _customerService.CreateContact(id, contact);
                return Ok(createdContact.Id);
            }
            catch (FluentValidationException ex)
            {
                ex.ValidationResult.AddToModelState(ModelState, null);
                return new BadRequestObjectResult(ModelState);
            }
        }

        /// <summary>
        /// Updates an existing contact.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <param name="patch">The patch operation.</param>
        /// <response code="200">Contact successfully updated</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Contact not found</response>
        [HttpPatch("/customers/{id}/contacts/{contactId}")]
        public async Task<ActionResult<bool>> PatchContact(Guid id, Guid contactId, JsonPatchDocument<Contact> patch)
        {
            try
            {
                var result = await _customerService.PatchContact(id, contactId, patch);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (FluentValidationException ex)
            {
                ex.ValidationResult.AddToModelState(ModelState, null);
                return new BadRequestObjectResult(ModelState);
            }
        }

        /// <summary>
        /// Deletes a contact matching the supplied Id.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <response code="200">Contact successfully deleted</response>
        /// <response code="404">Contact not found</response>
        [HttpDelete("/customers/{id}/contacts/{contactId}")]
        public async Task<ActionResult<bool>> DeleteContact(Guid id, Guid contactId)
        {
            var result = await _customerService.DeleteContact(id, contactId);

            return Ok(result);
        }

        /// <summary>
        /// Creates a new delivery address for a contact.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <param name="deliveryAddress">The delivery address to create Id.</param>
        /// <response code="200">Delivery address successfully created</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Delivery address for contact not found</response>
        [HttpPost("/customers/{id}/contacts/{contactId}/deliveryAddresses")]
        public async Task<ActionResult<Guid>> CreateDeliveryAddress(Guid id, Guid contactId, DeliveryAddress deliveryAddress)
        {
            try
            {
                var result = await _customerService.CreateDeliveryAddress(id, contactId, deliveryAddress);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (FluentValidationException ex)
            {
                ex.ValidationResult.AddToModelState(ModelState, null);
                return new BadRequestObjectResult(ModelState);
            }
        }

        /// <summary>
        /// Updates an existing delivery address for a contact.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <param name="deliveryAddressId">The delivery address Id.</param>
        /// <param name="patch">The patch operation.</param>
        /// <response code="200">Delivery address successfully updated</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Delivery address for contact not found</response>
        [HttpPatch("/customers/{id}/contacts/{contactId}/deliveryAddresses/{deliveryAddressId}")]
        public async Task<ActionResult<bool>> UpdateDeliveryAddress(Guid id, Guid contactId, Guid deliveryAddressId, JsonPatchDocument<DeliveryAddress> patch)
        {
            try
            {
                var result = await _customerService.PatchDeliveryAddress(id, contactId, deliveryAddressId, patch);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (FluentValidationException ex)
            {
                ex.ValidationResult.AddToModelState(ModelState, null);
                return new BadRequestObjectResult(ModelState);
            }
        }

        /// <summary>
        /// Deletes a delivery address matching the supplied Id.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <param name="deliveryAddressId">The delivery address Id.</param>
        /// <response code="200">Delivery address successfully deleted</response>
        /// <response code="404">Delivery address for contact not found</response>
        /// <response code="409">Delivery address cannot be deleted</response>
        [HttpDelete("/customers/{id}/contacts/{contactId}/deliveryAddresses/{deliveryAddressId}")]
        public async Task<ActionResult<bool>> DeleteDeliveryAddress(Guid id, Guid contactId, Guid deliveryAddressId)
        {
            try
            {
                var result = await _customerService.DeleteDeliveryAddress(id, contactId, deliveryAddressId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Conflict(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new card token for a contact.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <param name="command">The create payment method command.</param>
        /// <response code="200">Card token successfully created</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Card token for contact not found</response>
        [HttpPost("/customers/{id}/contacts/{contactId}/cardTokens")]
        public async Task<ActionResult<CardToken>> CreateCardToken(Guid id, Guid contactId, CardPaymentRequest command)
        {
            try
            {
                var token = await _customerService.AddCardToken(id, contactId, command);
                if (token == null)
                    return NotFound();
                else
                    return token;
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Generate a client token needed for capturing card details.
        /// </summary>
        [HttpGet("/customers/cardTokens/clientToken")]
        public async Task<ActionResult<string>> GenerateClientToken()
        {
            var token = await _paymentGatewayService.GetClientToken();
            return new JsonResult(token);
        }

        /// <summary>
        /// Deletes a card token matching the supplied Id from the payment provider vault and the Fed repository.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <param name="cardTokenId">The card token Id.</param>
        /// <response code="200">Card token successfully deleted</response>
        /// <response code="404">Card token for contact not found</response>
        [HttpDelete("/customers/{id}/contacts/{contactId}/cardTokens/{cardTokenId}")]
        public async Task<ActionResult<bool>> DeleteCardToken(Guid id, Guid contactId, Guid cardTokenId)
        {
            try
            {
                return await _customerService.DeleteCardToken(id, contactId, cardTokenId);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Creates a new billing address for a contact.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <param name="billingAddress">The billing address to create Id.</param>
        /// <response code="200">Billing address successfully created</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Billing address for contact not found</response>
        [HttpPost("/customers/{id}/contacts/{contactId}/billingAddress")]
        public async Task<ActionResult<Guid>> CreateBillingAddress(Guid id, Guid contactId, BillingAddress billingAddress)
        {
            try
            {
                var result = await _customerService.CreateBillingAddress(id, contactId, billingAddress);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (FluentValidationException ex)
            {
                ex.ValidationResult.AddToModelState(ModelState, null);
                return new BadRequestObjectResult(ModelState);
            }
        }

        /// <summary>
        /// Updates an existing billing address for a contact.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <param name="billingAddressId">The billing address Id.</param>
        /// <param name="patch">The patch operation.</param>
        /// <response code="200">Billing address successfully updated</response>
        /// <response code="400">Validation failed</response>
        /// <response code="404">Billing address for contact not found</response>
        [HttpPatch("/customers/{id}/contacts/{contactId}/billingAddress/{billingAddressId}")]
        public async Task<ActionResult<bool>> UpdateBillingAddress(Guid id, Guid contactId, Guid billingAddressId, JsonPatchDocument<BillingAddress> patch)
        {
            try
            {
                var result = await _customerService.PatchBillingAddress(id, contactId, billingAddressId, patch);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (FluentValidationException ex)
            {
                ex.ValidationResult.AddToModelState(ModelState, null);
                return new BadRequestObjectResult(ModelState);
            }
        }

        /// <summary>
        /// Deletes a billing address matching the supplied Id.
        /// </summary>
        /// <param name="id">The customer Id.</param>
        /// <param name="contactId">The contact Id.</param>
        /// <param name="billingAddressId">The billing address Id.</param>
        /// <response code="200">Billing address successfully deleted</response>
        /// <response code="404">Billing address for contact not found</response>
        [HttpDelete("/customers/{id}/contacts/{contactId}/billingAddress/{billingAddressId}")]
        public async Task<ActionResult<bool>> DeleteBillingAddress(Guid id, Guid contactId, Guid billingAddressId)
        {
            try
            {
                var result = await _customerService.DeleteBillingAddress(id, contactId, billingAddressId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return Conflict(ex.Message);
            }
        }
    }
}