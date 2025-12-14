using Fed.Core.Common;
using Fed.Core.Common.Interfaces;
using Fed.Core.Entities;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Api.External.AzureStorage
{
    public class AzureTableService : IAzureTableService
    {
        private const string deliveriesPartitionKey = "deliveries";
        private const string pickOrderRowKey = "pickOrder";
        private const string productsPartitionKey = "products";
        private const string productOrderRowKey = "pickOrder";

        public class PickOrderEntity : TableEntity
        {
            public PickOrderEntity() : base(deliveriesPartitionKey, pickOrderRowKey)
            {
                PickOrder = "";
            }

            public PickOrderEntity(string pickOrder) : base(deliveriesPartitionKey, pickOrderRowKey)
            {
                PickOrder = pickOrder;
            }

            public string PickOrder { get; set; }
        }


        public class ProductOrderEntity : TableEntity
        {
            public ProductOrderEntity() : base(productsPartitionKey, productOrderRowKey)
            {
                ProductOrder = "";
            }

            public ProductOrderEntity(string productOrder) : base(productsPartitionKey, productOrderRowKey)
            {
                ProductOrder = productOrder;
            }

            public string ProductOrder { get; set; }
        }

        private readonly IAzureConfig _azureConfig;

        public AzureTableService(IAzureConfig azureConfig)
        {
            _azureConfig = azureConfig ?? throw new ArgumentNullException(nameof(azureConfig));
        }

        public async Task<DeliveryPickOrder> GetCurrentPickOrderAsync()
        {
            CloudTable table = GetTable();

            var readOps = TableOperation.Retrieve<PickOrderEntity>(deliveriesPartitionKey, pickOrderRowKey);
            var tableResult = await table.ExecuteAsync(readOps);

            return (tableResult.HttpStatusCode == 200 && tableResult.Result != null)
                ? JsonConvert.DeserializeObject<DeliveryPickOrder>((tableResult.Result as PickOrderEntity).PickOrder)
                : DeliveryPickOrder.Default;
        }

        public async Task SetPickOrderAsync(DeliveryPickOrder pickOrder)
        {
            CloudTable table = GetTable();

            var json = JsonConvert.SerializeObject(pickOrder);
            var entity = new PickOrderEntity(json);
            var updateOps = TableOperation.InsertOrReplace(entity);
            await table.ExecuteAsync(updateOps);
        }

        public async Task<List<String>> GetCurrentProductOrderAsync()
        {
            CloudTable table = GetTable();

            var readOps = TableOperation.Retrieve<ProductOrderEntity>(productsPartitionKey, pickOrderRowKey);
            var tableResult = await table.ExecuteAsync(readOps);

            return (tableResult.HttpStatusCode == 200 && tableResult.Result != null)
                ? JsonConvert.DeserializeObject<List<String>>((tableResult.Result as ProductOrderEntity).ProductOrder)
                : new List<String>();
        }

        public async Task SetProductOrderAsync(List<String> productCodes)
        {
            CloudTable table = GetTable();

            var json = JsonConvert.SerializeObject(productCodes);
            var entity = new ProductOrderEntity(json);
            var updateOps = TableOperation.InsertOrReplace(entity);
            await table.ExecuteAsync(updateOps);
        }

        private CloudTable GetTable()
        {
            var storageAccount = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(_azureConfig.StorageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("SimpleKeyValueStore");
            return table;
        }
    }
}