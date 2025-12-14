using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class CustomerMarketingAttributeService : ICustomerMarketingAttributeService
    {
        private readonly ICustomerMarketingAttributesHandler _customerMarketingAttributesHandler;

        public CustomerMarketingAttributeService(ICustomerMarketingAttributesHandler customerMarketingAttributesHandler)
        {
            _customerMarketingAttributesHandler = customerMarketingAttributesHandler;
        }

        public async Task<CustomerMarketingAttribute> GetCustomerMarketingAttributeAsync(Guid customerMarketingAttributeId)
        {
            var attribute = await _customerMarketingAttributesHandler.ExecuteAsync(new GetByIdQuery<CustomerMarketingAttribute>(customerMarketingAttributeId));
            if (attribute == null)
                throw new KeyNotFoundException($"Customer marketing attribute with Id {customerMarketingAttributeId} does not exist");
            return attribute;
        }

        public async Task<IList<CustomerMarketingAttribute>> GetAllCustomerMarketingAttributesAsync()
        {
            var attributes = await _customerMarketingAttributesHandler.ExecuteAsync(new GetAllQuery<CustomerMarketingAttribute>());
            return attributes;
        }

        public async Task<CustomerMarketingAttribute> CreateCustomerMarketingAttributeAsync(CustomerMarketingAttribute customerMarketingAttribute)
        {
            var newAttribute = new CustomerMarketingAttribute(Guid.NewGuid(), customerMarketingAttribute.Name, customerMarketingAttribute.Description);
            await _customerMarketingAttributesHandler.ExecuteAsync(new CreateCommand<CustomerMarketingAttribute>(newAttribute));
            return newAttribute;
        }

        public async Task<bool> UpdateCustomerMarketingAttributeAsync(CustomerMarketingAttribute customerMarketingAttribute) =>
            await _customerMarketingAttributesHandler.ExecuteAsync(new UpdateCommand<CustomerMarketingAttribute>(Guid.Empty, customerMarketingAttribute)); //UpdateCommand requires an id as argument

    }
}
