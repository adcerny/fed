using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class DeliveryBoundaryService : IDeliveryBoundaryService
    {
        private IDeliveryBoundaryHandler _deliveryBoundaryHandler;

        public DeliveryBoundaryService(
           IDeliveryBoundaryHandler deliveryBoundaryHandler)
        {
            _deliveryBoundaryHandler = deliveryBoundaryHandler ?? throw new ArgumentNullException(nameof(deliveryBoundaryHandler));
        }

        public async Task<IList<DeliveryBoundary>> GetDeliveryBoundaryAsync()
        {
            return await _deliveryBoundaryHandler.ExecuteAsync(new GetAllQuery<DeliveryBoundary>());
        }
    }
}