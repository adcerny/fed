using Fed.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IDeliveryBoundaryService
    {
        Task<IList<DeliveryBoundary>> GetDeliveryBoundaryAsync();
    }
}
