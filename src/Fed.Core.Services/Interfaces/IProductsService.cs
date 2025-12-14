using Fed.Core.Entities;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IProductsService
    {
        Task<IList<Product>> GetProducts(string productGroup, Guid? productCategoryId = null);

        Task<Product> GetProduct(string timeslotId);

        Task<Product> CreateProduct(Product product);

        Task<bool> DeleteProduct(string timeslotId);

        Task PatchProduct(string id, JsonPatchDocument<Product> patch);
    }
}
