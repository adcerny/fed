using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IList<GeneratedOrder>> GenerateOrdersAsync(Date date);
        Task<IList<Order>> GetOrdersAsync(Date date, Guid? contactId, bool excludeUnpaid);
        Task<Order> GetOrderAsync(Guid Id);
        Task<IList<OrderSummary>> GetOrderSummaryAsync(Date fromDate, Date toDate, Guid contactId);
        Task<OrderDeliveryContext<Order>> GetOrderForDateAsync(Guid orderId, Date deliveryDate);
    }
}