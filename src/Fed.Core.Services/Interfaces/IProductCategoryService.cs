using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Core.Services.Interfaces
{
    public interface IProductCategoryService
    {
        Task<ProductCategory> GetProductCategoryAsync(Guid categoryId);
        Task<IList<ProductCategory>> GetAllProductCategoriesAsync();
        Task<ProductCategory> CreateProductCategoryAsync(ProductCategory category);
        Task<bool> UpdateProductCategoryAsync(ProductCategory attribute);
    }
}
