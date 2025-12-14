using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Core.Services
{
    public class ProductCategoryService : IProductCategoryService
    {
        private readonly IProductCategoriesHandler _productCategoriesHandler;

        public ProductCategoryService(IProductCategoriesHandler productCategoriesHandler)
        {
            _productCategoriesHandler = productCategoriesHandler;
        }

        public async Task<ProductCategory> CreateProductCategoryAsync(ProductCategory category)
        {
            var productCategory = new ProductCategory(Guid.NewGuid(), category.Name);
            bool created = await _productCategoriesHandler.ExecuteAsync(new CreateCommand<ProductCategory>(productCategory));
            return created ? productCategory : null;
        }

        public async Task<IList<ProductCategory>> GetAllProductCategoriesAsync()
        {
            var categories = await _productCategoriesHandler.ExecuteAsync(new GetAllQuery<ProductCategory>());
            return categories;
        }

        public async Task<ProductCategory> GetProductCategoryAsync(Guid categoryId)
        {
            var category = await _productCategoriesHandler.ExecuteAsync(new GetByIdQuery<ProductCategory>(categoryId));
            if (category == null)
                throw new KeyNotFoundException($"Product Category with Id {categoryId} does not exist");
            return category;
        }

        public async Task<bool> UpdateProductCategoryAsync(ProductCategory category) =>
            await _productCategoriesHandler.ExecuteAsync(new UpdateCommand<ProductCategory>(Guid.Empty, category));
    }
}
