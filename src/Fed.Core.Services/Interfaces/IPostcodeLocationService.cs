using Fed.Core.Entities;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IPostcodeLocationService
    {
        Task<PostcodeLocation> GetPostcodeLocation(string postcode);
    }
}
