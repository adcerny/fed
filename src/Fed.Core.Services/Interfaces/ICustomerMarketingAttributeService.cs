using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface ICustomerMarketingAttributeService
    {
        Task<CustomerMarketingAttribute> GetCustomerMarketingAttributeAsync(Guid attributeId);
        Task<IList<CustomerMarketingAttribute>> GetAllCustomerMarketingAttributesAsync();
        Task<CustomerMarketingAttribute> CreateCustomerMarketingAttributeAsync(CustomerMarketingAttribute attribute);
        Task<bool> UpdateCustomerMarketingAttributeAsync(CustomerMarketingAttribute attribute);
    }
}
