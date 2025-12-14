using Fed.Core.Data.Commands;
using Fed.Core.Data.Handlers;
using Fed.Core.Data.Queries;
using Fed.Core.Entities;
using Fed.Core.Services.Interfaces;
using Microsoft.AspNetCore.JsonPatch;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Core.Services
{

    public class ProductsService : IProductsService
    {
        private readonly IProductsHandler _handler;

        public ProductsService(IProductsHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public async Task<IList<Product>> GetProducts(string productGroup, Guid? productCategoryId = null)
        {
            var query = new GetProductsQuery(productGroup, productCategoryId);
            var products = await _handler.ExecuteAsync(query);
            return products;
        }

        public async Task<Product> GetProduct(string id)
        {
            var product = await _handler.ExecuteAsync(new GetByIdQuery<Product>(id));

            if (product == null)
                throw new KeyNotFoundException($"Product with ID {id} not found.");

            return product;
        }

        public async Task<Product> CreateProduct(Product product)
        {
            await _handler.ExecuteAsync(new CreateCommand<Product>(product));
            return await GetProduct(product.Id);
        }

        public async Task PatchProduct(string id, JsonPatchDocument<Product> patch)
        {
            var getProductQuery = new GetByIdQuery<Product>(id);

            var product = await _handler.ExecuteAsync(getProductQuery);

            if (product == null)
                throw new KeyNotFoundException();

            patch.ApplyTo(product);

            var updateCommand = new UpdateCommand<Product>(product.Id, product);

            await _handler.ExecuteAsync(updateCommand);
        }

        public async Task<bool> DeleteProduct(string id)
        {
            bool result = await _handler.ExecuteAsync(new DeleteCommand<string>(id));
            return result;
        }
    }
}
