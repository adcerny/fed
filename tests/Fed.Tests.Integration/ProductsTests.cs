using Fed.Core.Entities;
using Fed.Tests.Common;
using Fed.Web.Service.Client;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Fed.Tests.Integration
{
    public class ProductsTests
    {
        [Fact]
        public async Task GetProductsForCategoryWhichDoesntExist()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var products = await fedClient.GetProductsAsync(Guid.NewGuid());

            Assert.Equal(0, products.Count);
        }

        [Fact]
        public async Task GetProducts()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var products = await fedClient.GetProductsAsync(null);

            Assert.True(products.Count > 100);
        }

        [Fact]
        public async Task GetProductById()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var products = await fedClient.GetProductsAsync(null);

            var id = products.FirstOrDefault().Id;

            var product = await fedClient.GetProductAsync(id);

            Assert.Equal(id, product.Id);

        }

        [Fact]
        public async Task CreateProduct()
        {
            const int childCount = 4;
            string productName = TestDataBuilder.GetRandomString();

            var fedClient = FedWebClient.Create(new NullLogger());
            var products = await fedClient.GetProductsAsync(null);

            var newProduct = TestDataBuilder.BuildProduct(products.Take(childCount).ToList());
            newProduct.ProductName = productName;

            var createdProduct = await fedClient.CreateProductAsync(newProduct);

            Assert.Equal(productName, createdProduct.ProductName);

            Assert.Equal(childCount, createdProduct.ChildProducts.Count);

            //tidy up
            await fedClient.DeleteProductAsync(createdProduct.Id);
        }

        [Fact]
        public async Task UpdateProduct()
        {
            const int childCount = 7;

            var fedClient = FedWebClient.Create(new NullLogger());
            var products = await fedClient.GetProductsAsync(null);

            var newProduct = TestDataBuilder.BuildProduct();
            string productName = newProduct.ProductName;

            var createdProduct = await fedClient.CreateProductAsync(newProduct);

            Assert.Equal(productName, createdProduct.ProductName);
            Assert.Equal(0, (createdProduct.ChildProducts?.Count ?? 0));

            var updatedName = TestDataBuilder.GetRandomName();
            var updatedChildren = products.Take(childCount).ToList();

            var patchName = PatchOperation.CreateReplace("/productName", updatedName);
            var patchChildren = PatchOperation.CreateReplace("/childProducts", updatedChildren);

            await fedClient.PatchProductAsync(createdProduct.Id, patchName, patchChildren);

            var updatedProduct = await fedClient.GetProductAsync(createdProduct.Id);

            Assert.Equal(updatedName, updatedProduct.ProductName);
            Assert.Equal(childCount, updatedProduct.ChildProducts.Count);

            //tidy up
            await fedClient.DeleteProductAsync(createdProduct.Id);
        }
    }
}
