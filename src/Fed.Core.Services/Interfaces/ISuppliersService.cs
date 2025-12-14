using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface ISuppliersService
    {
        Task<IList<Supplier>> GetSuppliersAsync();

        Task<Supplier> GetSupplierAsync(int Id);

        Task<Supplier> CreateSupplierAsync(Supplier supplier);

        Task<bool> UpdateSupplierAsync(int id, Supplier supplier);

        Task<IList<SupplierProductQuantity>> GetConfirmedSupplierQuantitiesAsync(int supplierId, Date deliveryDate);

        Task<IDictionary<Date, IList<SupplierProductQuantity>>> GetSupplierForecastAsync(
            int supplierId, Date toDate, bool excludeFedBuffer = false);
    }
}