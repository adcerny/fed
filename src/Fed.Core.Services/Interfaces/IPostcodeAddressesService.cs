using Fed.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IPostcodeAddressesService
    {
        Task<List<Address>> GetAddresses(string postcode);
        Task<List<DeliverableAddress>> GetDeliverableAddresses(string postcode);
    }
}
