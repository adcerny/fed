using Fed.Core.Entities;
using Fed.Tests.Common;
using Fed.Web.Service.Client;
using System.Threading.Tasks;
using Xunit;

namespace Fed.Tests.Integration
{
    public class SuppliersTests
    {
        [Fact]
        public async Task CRUDSupplierTest()
        {
            var fedClient = FedWebClient.Create(new NullLogger());

            var suppliers = await fedClient.GetSuppliersAsync();

            Assert.NotEmpty(suppliers);

            string name = TestDataBuilder.GetRandomCompanyName();

            Supplier newSupplier = new Supplier(0, name);

            var createdSupplier = await fedClient.CreateSupplierAsync(newSupplier);

            Assert.Equal(name, createdSupplier.Name);

            string newName = TestDataBuilder.GetRandomCompanyName();

            var patchName = PatchOperation.CreateReplace("/name", newName);

            await fedClient.PatchSupplierAsync(createdSupplier.Id, patchName);

            var updatedSupplier = await fedClient.GetSupplierAsync(createdSupplier.Id);

            Assert.Equal(newName, updatedSupplier.Name);

        }
    }
}
