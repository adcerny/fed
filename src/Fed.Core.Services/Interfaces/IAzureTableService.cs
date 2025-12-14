using Fed.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Api.External.AzureStorage
{
    public interface IAzureTableService
    {
        Task<DeliveryPickOrder> GetCurrentPickOrderAsync();

        Task SetPickOrderAsync(DeliveryPickOrder pickOrder);

        Task<List<String>> GetCurrentProductOrderAsync();

        Task SetProductOrderAsync(List<String> productCodes);
    }
}