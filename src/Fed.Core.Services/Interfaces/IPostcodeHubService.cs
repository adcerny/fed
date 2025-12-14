using System;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IPostcodeHubService
    {
        Task<Guid> GetHubIdForPostcode(string postcode);
    }
}
