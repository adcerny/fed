using Fed.Core.Entities;
using Fed.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IBakeryMinimumOrderService
    {
        decimal MinimumOrderValue { get; }

        decimal GetSupplierPrice(string sku);
        Task<IList<(Product, int)>> CalculateAdditionalRequiredBreadStockAsync(IList<SupplierProductQuantity> currentOrders);
        Task TopUpBreadOrderIfNeeded(IList<SupplierProductQuantity> currentOrders);
    }
}