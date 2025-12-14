using Fed.Core.Entities;
using Fed.Core.Enums;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IDeliveryService
    {
        // Deliveries
        Task<IList<Delivery>> GetDeliveriesAsync(Date date);
        Task<Delivery> GetDeliveryAsync(string id);
        Task<IList<Delivery>> CreateDeliveriesAsync(Date date);
        Task<bool> DeleteDeliveriesAsync(Date date);
        Task<bool> SetDeliveryPackagingStatusAsync(Guid deliveryId, PackingStatus packagingStatus);
        Task<bool> SetDeliveryBagCountAsync(string deliveryId, int bagCount);

        // Delivery Shortages
        Task<IList<DeliveryShortage>> GetDeliveryShortagesAsync(Date date);
        Task<DeliveryShortage> ShortDeliveryItemAsync(Guid orderId, string productId, int actualQuantity, decimal productPrice, string reason, string reasonCode);
        Task DeleteDeliveryShortageAsync(Guid deliveryShortageId);

        // Delivery Additions
        Task<IList<DeliveryAddition>> GetDeliveryAdditionsAsync(Date date);
        Task<DeliveryAddition> AddSubstituteToDeliveryAsync(Guid orderId, string productId, int quantity, string notes, Guid deliveryShortageId);
        Task DeleteDeliveryAdditionAsync(Guid deliveryAdditionId);
    }
}