using Fed.Core.Entities;
using Fed.Core.Models;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<Customer> GetCustomer(Guid id);

        Task<IList<Customer>> GetCustomers(bool includeContacts);

        Task<FullCustomerInfo> GetFullCustomerInfo(string id);

        Task<Contact> GetContact(Guid customerId, Guid contactId);

        Task<Customer> CreateCustomer(Customer customer);

        Task<Contact> CreateContact(Guid customerId, Contact contact);

        Task<bool> PatchCustomer(Guid id, JsonPatchDocument<Customer> patch);

        Task<bool> PatchContact(Guid id, Guid contactId, JsonPatchDocument<Contact> patch);

        Task<bool> DeleteContact(Guid id, Guid contactId);

        Task<Guid> CreateDeliveryAddress(Guid id, Guid contactId, DeliveryAddress deliveryAddress);

        Task<bool> PatchDeliveryAddress(Guid id, Guid contactId, Guid deliveryAddressId, JsonPatchDocument<DeliveryAddress> patch);

        Task<Guid> CreateBillingAddress(Guid id, Guid contactId, BillingAddress billingAddress);     

        Task<bool> PatchBillingAddress(Guid id, Guid contactId, Guid billingAddressId, JsonPatchDocument<BillingAddress> patch);

        Task<CardToken> AddCardToken(Guid customerId, Guid contactId, CardPaymentRequest command);

        Task<bool> DeleteCardToken(Guid customerId, Guid contactId, Guid cardTokenId);

        Task<bool> DeleteDeliveryAddress(Guid customerId, Guid contactId, Guid deliveryAddressId);

        Task<bool> DeleteBillingAddress(Guid id, Guid contactId, Guid billingAddressId);
    }
}
