using Fed.AzureFunctions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class GetCustomerActivityFunction
    {
        [FunctionName("GetCustomerActivityFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger logger)
        {
            var cloudTable = GetTable("CustomerActivity");
            string userId = req.Query["userId"];
            if (string.IsNullOrEmpty(userId))
            {
                logger.LogError("userId cannot be null or empty");
                return new BadRequestObjectResult("userId cannot be null or empty");
            }

            if (!Enum.TryParse(req.Query["activityType"], out ActivityType activityType))
            {
                string typeList = string.Join(", ", Enum.GetNames(typeof(ActivityType)));
                logger.LogError($"Invalid activity type. Valid values are {typeList}");
                return new BadRequestObjectResult($"Invalid activity type. Valid values are {typeList}");
            }

            var results = new List<CustomerActivityEntity>();

            var query =
                new TableQuery<CustomerActivityEntity>().Where(
                    TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition(
                            "PartitionKey",
                            QueryComparisons.Equal, activityType.ToString()),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("UserId", QueryComparisons.Equal, userId)));

            TableContinuationToken token = null;

            do
            {
                TableQuerySegment<CustomerActivityEntity> resultSegment = await cloudTable.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;
                results.AddRange(resultSegment.Results);
            } while (token != null);

            return new JsonResult(results);
        }

        private static CloudTable GetTable(string tableName)
        {
            var conn = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
            var account = CloudStorageAccount.Parse(conn);
            var client = account.CreateCloudTableClient();
            var table = client.GetTableReference(tableName);
            table.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            return table;
        }
    }
}
