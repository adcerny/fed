using Fed.Core.Entities;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface ICRMService
    {
        Task<bool> AddLead(Customer customer);
    }
}
