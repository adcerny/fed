using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface ICustomerAgentService
    {
        Task<CustomerAgent> GetCustomerAgentAsync(Guid agentId);
        Task<IList<CustomerAgent>> GetAllCustomerAgentsAsync();
        Task<CustomerAgent> CreateCustomerAgentAsync(CustomerAgent agent);
        Task<bool> DeleteCustomerAgentAsync(Guid agentId);
    }
}
