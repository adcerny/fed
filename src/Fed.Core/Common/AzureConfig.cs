using Fed.Core.Common.Interfaces;
using System;

namespace Fed.Core.Common
{
    public class AzureConfig : IAzureConfig
    {
        public AzureConfig(string storageConnectionString)
        {
            StorageConnectionString = storageConnectionString ?? throw new ArgumentNullException(nameof(storageConnectionString));
        }

        public string StorageConnectionString { get; }
    }
}
