using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class CustomerAgentService : ICustomerAgentService
    {
        private readonly ICustomerAgentsHandler _customerAgentsHandler;

        public CustomerAgentService(
            ICustomerAgentsHandler customerAgentsHandler)
        {
            _customerAgentsHandler = customerAgentsHandler;
        }

        public async Task<CustomerAgent> GetCustomerAgentAsync(Guid agentId)
        {
            var agent = await _customerAgentsHandler.ExecuteAsync(new GetByIdQuery<CustomerAgent>(agentId));
            if (agent == null)
                throw new KeyNotFoundException($"Agent with Id {agentId} does not exist");
            return agent;
        }

        public async Task<IList<CustomerAgent>> GetAllCustomerAgentsAsync()
        {
            return await _customerAgentsHandler.ExecuteAsync(new GetAllQuery<CustomerAgent>());
        }

        public async Task<CustomerAgent> CreateCustomerAgentAsync(CustomerAgent customerAgent)
        {
            var id = Guid.NewGuid();

            TextInfo ti = new CultureInfo("en-GB", false).TextInfo;
            var name = ti.ToTitleCase(customerAgent.Name);

            var newAgent = new CustomerAgent(id, name, customerAgent.Email);
            await _customerAgentsHandler.ExecuteAsync(new CreateCommand<CustomerAgent>(newAgent));
            return newAgent;
        }

        public async Task<bool> DeleteCustomerAgentAsync(Guid agentId)
        {
            return await _customerAgentsHandler.ExecuteAsync(new DeleteCommand<CustomerAgent>(agentId));
        }
    }
}
