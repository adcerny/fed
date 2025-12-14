using Fed.AzureFunctions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class LogCustomerActivityFunction
    {
        [FunctionName("LogCustomerActivityFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Table("CustomerActivity")] CloudTable cloudTable,
            ILogger logger)
        {

            string userId = req.Query["userId"];
            if (string.IsNullOrEmpty(userId))
            {
                logger.LogError("userId cannot be null or empty");
                return new BadRequestObjectResult("userId cannot be null or empty");
            }

            string ipAddress = req.Query["ipAddress"];
            if (string.IsNullOrEmpty(ipAddress))
            {
                logger.LogError("IpAddress cannot be null or empty");
                return new BadRequestObjectResult("IpAddress cannot be null or empty");
            }

            string userAgent = req.Query["userAgent"];
            if (string.IsNullOrEmpty(userAgent))
            {
                logger.LogError("UserAgent cannot be null or empty");
                return new BadRequestObjectResult("UserAgent cannot be null or empty");
            }

            ActivityType activityType;
            if (!Enum.TryParse(req.Query["activityType"], out activityType))
            {
                string typeList = string.Join(", ", Enum.GetNames(typeof(ActivityType)));
                logger.LogError($"Invalid activity type. Valid values are {typeList}");
                return new BadRequestObjectResult($"Invalid activity type. Valid values are {typeList}");
            }


            string metaData = req.Query["metaData"];

            var entity = new CustomerActivityEntity(activityType, userId, ipAddress, userAgent, metaData);
            var updateOps = TableOperation.InsertOrReplace(entity);
            await cloudTable.ExecuteAsync(updateOps);

            return new OkResult();
        }
    }
}
