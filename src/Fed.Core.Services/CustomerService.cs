using Fed.Api.External.AzureStorage;
using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.Exceptions;
using Fed.Core.Extensions;
using Fed.Core.Models;
using Fed.Core.Services.Interfaces;
using Fed.Core.Services.Validators;
using Fed.Core.ValueTypes;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomersHandler _customersHandler;
        private readonly IAzureQueueService _azureQueueService;
        private readonly IContactsHandler _contactsHandler;
        private readonly IDeliveryAddressHandler _deliveryAddressHandler;
        private readonly ICardTokenHandler _cardTokenHandler;
        private readonly IBillingAddressHandler _billingAddressHandler;
        private readonly IPaymentGatewayService _paymentGatewayService;
        private readonly IRecurringOrderService _recurringOrderService;
        private readonly IDiscountService _discountService;

        public CustomerService(
            ICustomersHandler customersHandler,
            IContactsHandler contactssHandler,
            IDeliveryAddressHandler deliveryAddressHandler,
            IBillingAddressHandler billingAddressHandler,
            ICardTokenHandler cardTokenHandler,
            IPaymentGatewayService paymentGatewayService,
            IRecurringOrderService recurringOrderService,
            IAzureQueueService azureQueueService,
            IDiscountService discountService)
        {
            _azureQueueService = azureQueueService ?? throw new ArgumentNullException(nameof(azureQueueService));
            _customersHandler = customersHandler ?? throw new ArgumentNullException(nameof(customersHandler));
            _contactsHandler = contactssHandler ?? throw new ArgumentNullException(nameof(contactssHandler));
            _cardTokenHandler = cardTokenHandler ?? throw new ArgumentNullException(nameof(cardTokenHandler));
            _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
            _deliveryAddressHandler = deliveryAddressHandler ?? throw new ArgumentNullException(nameof(deliveryAddressHandler));
            _billingAddressHandler = billingAddressHandler ?? throw new ArgumentNullException(nameof(billingAddressHandler));
            _recurringOrderService = recurringOrderService ?? throw new ArgumentNullException(nameof(recurringOrderService));
            _discountService = discountService ?? throw new ArgumentNullException(nameof(discountService));
        }


        public async Task<Customer> GetCustomer(Guid id)
        {
            var getCustomerQuery = new GetByIdQuery<Customer>(id);

            var customer = await _customersHandler.ExecuteAsync(getCustomerQuery);

            if (customer == null)
                throw new KeyNotFoundException($"No customer found with ID {id}.");

            return customer;
        }

        public async Task<IList<Customer>> GetCustomers(bool includeContacts)
        {
            var result = await _customersHandler.ExecuteAsync(new GetCustomersQuery(includeContacts));

            return result;
        }

        public async Task<FullCustomerInfo> GetFullCustomerInfo(string id)
        {
            var customer =
                await _customersHandler.ExecuteAsync(
                    new GetByIdQuery<FullCustomerInfo>(id));

            if (customer == null)
                throw new KeyNotFoundException($"No customer found with ID {id}.");

            return customer;
        }

        public async Task<Customer> CreateCustomer(Customer customer)
        {

            var results = new CustomerValidator().Validate(customer);
            if (!results.IsValid)
                throw new FluentValidationException(results);

            var command = new CreateCommand<Customer>(customer);
            try
            {
                var newCustomer = await _customersHandler.ExecuteAsync(command);
                var createdContact = newCustomer.Contacts.First();

                if (newCustomer.AccountType != AccountType.Deleted && newCustomer.AccountType != AccountType.Internal)
                {
                    await _azureQueueService.SyncContactWithSendGridAsync(
                    createdContact.Email,
                    createdContact.Email,
                    newCustomer.ShortId,
                    createdContact.ShortId,
                    createdContact.BillingAddresses?.FirstOrDefault()?.CompanyName,
                    createdContact.FirstName,
                    createdContact.LastName);

                    if (!createdContact.IsMarketingConsented)
                        await _azureQueueService.SyncSubscriberWithSendGridAsync(createdContact.Email, false);
                }
                await _discountService.ApplyDiscounts(newCustomer.Id, DiscountEvent.SignUp);

                return newCustomer;
            }
            catch (DuplicateEmailAddresssException ex)
            {
                var existingCustomer = await _customersHandler.ExecuteAsync(new GetByIdQuery<Customer>(ex.ExistingCustomerId));
                if (existingCustomer.AccountType == AccountType.Demo)
                {
                    customer.Notes = existingCustomer.Notes;
                    customer.CustomerMarketingAttributeId = existingCustomer.CustomerMarketingAttributeId;
                    customer.CustomerAgentId = existingCustomer.CustomerAgentId;
                    await _customersHandler.ExecuteAsync(new UpdateCommand<Customer>(existingCustomer.Id, customer));
                    await _contactsHandler.ExecuteAsync(new UpdateCommand<Contact>(existingCustomer.PrimaryContact.Id, customer.Contacts.FirstOrDefault()));
                    return await _customersHandler.ExecuteAsync(new GetByIdQuery<Customer>(existingCustomer.Id));
                }
                else
                    throw ex;
            }
        }

        public async Task<Contact> CreateContact(Guid customerId, Contact contact)
        {
            var results = new ContactValidator().Validate(contact);
            if (!results.IsValid)
                throw new FluentValidationException(results);

            var command = new CreateContactCommand(customerId, contact);

            var result = await _contactsHandler.ExecuteAsync(command);

            var customer = await _customersHandler.ExecuteAsync(new GetByIdQuery<Customer>(customerId));
            var createdContact = customer.Contacts.Single(c => c.Id.Equals(result));

            if (customer.AccountType != AccountType.Deleted && customer.AccountType != AccountType.Internal)
            {
                await _azureQueueService.SyncContactWithSendGridAsync(
                createdContact.Email,
                createdContact.Email,
                customer.ShortId,
                createdContact.ShortId,
                createdContact.BillingAddresses?.FirstOrDefault()?.CompanyName,
                createdContact.FirstName,
                createdContact.LastName);

                if (!createdContact.IsMarketingConsented)
                    await _azureQueueService.SyncSubscriberWithSendGridAsync(createdContact.Email, false);
            }
            return createdContact;
        }

        public async Task<bool> PatchCustomer(Guid id, JsonPatchDocument<Customer> patch)
        {
            var getCustomerQuery = new GetByIdQuery<Customer>(id);

            var customer = await _customersHandler.ExecuteAsync(getCustomerQuery);

            if (customer == null)
                throw new KeyNotFoundException($"No customer found with ID {id}.");

            patch.ApplyTo(customer);

            var results = new CustomerValidator().Validate(customer);
            if (!results.IsValid)
                throw new FluentValidationException(results);

            var updateCommand = new UpdateCommand<Customer>(customer.Id, customer);

            bool result = await _customersHandler.ExecuteAsync(updateCommand);

            return result;
        }

        public async Task<bool> PatchContact(Guid id, Guid contactId, JsonPatchDocument<Contact> patch)
        {
            var customer = await _customersHandler.ExecuteAsync(new GetByIdQuery<Customer>(id));
            if (customer == null)
                throw new KeyNotFoundException($"No customer found with ID {id}.");

            var contact = customer.Contacts.Single(c => c.Id.Equals(contactId));
            var prePatchEmail = contact.Email;


            if (contact == null)
                throw new KeyNotFoundException($"No contact found with ID {id} for customer with ID {id}.");

            var prePatchMarketingConsent = contact.IsMarketingConsented;

            patch.ApplyTo(contact);

            var postPatchMarketingConsent = contact.IsMarketingConsented;

            var results = new ContactValidator().Validate(contact);
            if (!results.IsValid)
                throw new FluentValidationException(results);

            var updateCommand = new UpdateCommand<Contact>(contactId, contact);

            await _contactsHandler.ExecuteAsync(updateCommand);
            if (customer.AccountType != AccountType.Deleted && customer.AccountType != AccountType.Internal)
            {
                await _azureQueueService.SyncContactWithSendGridAsync(
                prePatchEmail,
                contact.Email,
                customer.ShortId,
                contact.ShortId,
                contact.BillingAddresses?.FirstOrDefault()?.CompanyName,
                contact.FirstName,
                contact.LastName);

                if (prePatchMarketingConsent != postPatchMarketingConsent || prePatchEmail.ToLower() != contact.Email.ToLower())
                    await _azureQueueService.SyncSubscriberWithSendGridAsync(contact.Email, postPatchMarketingConsent);
            }

            return true;
        }

        public async Task<bool> DeleteContact(Guid id, Guid contactId)
        {
            var contact = await GetContact(id, contactId);
            if(contact == null)
                throw new KeyNotFoundException($"No contact found with ID {id} for customer with ID {id}.");

            var deleteContactCommand = new DeleteCommand<Contact>(id);

            var result = await _contactsHandler.ExecuteAsync(deleteContactCommand);

            return result;
        }

        public async Task<Contact> GetContact(Guid customerId, Guid contactId)
        {
            var getContactQuery = new GetByIdQuery<Contact>(contactId);

            var contact = await _contactsHandler.ExecuteAsync(getContactQuery);

            if (contact?.CustomerId != customerId)
                return null;

            return contact?.CustomerId == customerId ? contact : null;
        }

        public async Task<Guid> CreateDeliveryAddress(Guid id, Guid contactId, DeliveryAddress deliveryAddress)
        {
            var contact = await GetContact(id, contactId);

            var results = new DeliveryAddressValidator().Validate(deliveryAddress);
            if (!results.IsValid)
                throw new FluentValidationException(results);

            deliveryAddress.IsPrimary = deliveryAddress.IsPrimary || (!contact.DeliveryAddresses?.Any() ?? true);

            var createCommand = new CreateDeliveryAddressCommand(contactId, deliveryAddress);

            var result = await _deliveryAddressHandler.ExecuteAsync(createCommand);

            return result;
        }

        public async Task<bool> PatchDeliveryAddress(Guid id, Guid contactId, Guid deliveryAddressId, JsonPatchDocument<DeliveryAddress> patch)
        {
            var contact = await GetContact(id, contactId);

            var deliveryAddress = contact?.DeliveryAddresses?.Where(c => c.Id == deliveryAddressId).FirstOrDefault();

            if (deliveryAddress == null)
                throw new KeyNotFoundException($"No delivery address found with ID {deliveryAddressId} for contact with ID {contactId}.");

            patch.ApplyTo(deliveryAddress);

            var results = new DeliveryAddressValidator().Validate(deliveryAddress);
            if (!results.IsValid)
                throw new FluentValidationException(results);

            var updateCommand = new UpdateCommand<DeliveryAddress>(deliveryAddressId, deliveryAddress);

            var result = await _deliveryAddressHandler.ExecuteAsync(updateCommand);

            return result;
        }

        public async Task<Guid> CreateBillingAddress(Guid id, Guid contactId, BillingAddress billingAddress)
        {
            var contact = await GetContact(id, contactId);

            var results = new BillingAddressValidator().Validate(billingAddress);
            if (!results.IsValid)
                throw new FluentValidationException(results);

            billingAddress.IsPrimary = billingAddress.IsPrimary || (!contact.BillingAddresses?.Any() ?? true);

            var createCommand = new CreateBillingAddressCommand(contactId, billingAddress);

            var result = await _billingAddressHandler.ExecuteAsync(createCommand);

            return result;
        }

        public async Task<bool> PatchBillingAddress(Guid id, Guid contactId, Guid billingAddressId, JsonPatchDocument<BillingAddress> patch)
        {
            var contact = await GetContact(id, contactId);

            var billingAddress = contact?.BillingAddresses?.Where(b => b.Id == billingAddressId)
                                         .FirstOrDefault();

            if (billingAddress == null)
                throw new KeyNotFoundException($"No billing address found with ID {billingAddressId} for contact with ID {contactId}.");

            patch.ApplyTo(billingAddress);

            var results = new BillingAddressValidator().Validate(billingAddress);
            if (!results.IsValid)
                throw new FluentValidationException(results);

            var updateCommand = new UpdateCommand<BillingAddress>(billingAddressId, billingAddress);

            var result = await _billingAddressHandler.ExecuteAsync(updateCommand);

            return result;
        }

        public async Task<CardToken> AddCardToken(Guid customerId, Guid contactId, CardPaymentRequest command)
        {
            var contact = await GetContact(customerId, contactId);

            if (contact == null)
                return null;

            var cardToken = _paymentGatewayService.CreateCard(contact, command);

            //remove any other card tokens - remove this when we are able to store multiple payment methods
            foreach (var ct in contact.CardTokens.OrEmptyIfNull())
                await DeleteCardToken(customerId, contactId, ct.Id);

            cardToken.IsPrimary = true;
            var createCommand = new CreateCardTokenCommand(contactId, cardToken);
            var result = await _cardTokenHandler.ExecuteAsync(createCommand);
            if (contact.PaymentMethod != PaymentMethod.Card)
            {
                contact.PaymentMethod = PaymentMethod.Card;
                await _contactsHandler.ExecuteAsync(new UpdateCommand<Contact>(contact.Id, contact));
            }

            return cardToken;
        }

        public async Task<bool> DeleteCardToken(Guid customerId, Guid contactId, Guid cardTokenId)
        {
            var contact = await GetContact(customerId, contactId);

            var cardToken = contact?.CardTokens?.Where(c => c.Id == cardTokenId).FirstOrDefault();

            if (cardToken == null)
                return false;

            _paymentGatewayService.DeleteCardToken(cardToken);

            var deleteCommand = new DeleteCommand<CardToken>(cardTokenId);
            return await _cardTokenHandler.ExecuteAsync(deleteCommand);
        }

        public async Task<bool> DeleteDeliveryAddress(Guid customerId, Guid contactId, Guid deliveryAddressId)
        {
            var contact = await GetContact(customerId, contactId);

            var deliveryAddress = contact?.DeliveryAddresses?.Where(c => c.Id == deliveryAddressId).FirstOrDefault();

            if (deliveryAddress == null)
                throw new KeyNotFoundException("The delivery address does not exist");

            if (deliveryAddress.IsPrimary)
                throw new ArgumentException("Cannot delete primary delivery address");

            if (contact.DeliveryAddresses.Count == 1)
                throw new ArgumentException("Contact must have at least one delivery address");

            var recurringOrders = await _recurringOrderService.GetRecurringOrdersAsync(contact.Id, DateRange.TodayUntilEnd(), true);

            if (recurringOrders != null && recurringOrders.Any(o => o.DeliveryAddressId == deliveryAddressId))
                throw new ArgumentException("Cannot delete a delivery address that has active orders");

            var deleteCommand = new DeleteCommand<DeliveryAddress>(deliveryAddressId);

            var result = await _deliveryAddressHandler.ExecuteAsync(deleteCommand);
            return result;
        }

        public async Task<bool> DeleteBillingAddress(Guid id, Guid contactId, Guid billingAddressId)
        {
            var contact = await GetContact(id, contactId);

            var billingAddress = contact?.BillingAddresses?.Where(b => b.Id == billingAddressId)
                                         .FirstOrDefault();
            if (billingAddress == null)
                throw new KeyNotFoundException("The billing address does not exist");

            if (billingAddress.IsPrimary)
                throw new ArgumentException("Cannot delete primary billing address");

            var deleteCommand = new DeleteCommand<BillingAddress>(billingAddressId);
            var result = await _billingAddressHandler.ExecuteAsync(deleteCommand);

            return result;
        }
    }
}
