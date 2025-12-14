using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class AbelAndColeStaffingForecastFunction
    {
        private const string FuncName = "AbelAndColeStaffingForecastFunction";
        private const string Schedule = "0 0 15 * * 1-5";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timerInfo,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, NotifyAbelAndColeOpsTeam);

        public static async Task NotifyAbelAndColeOpsTeam(ServicesBag bag)
        {
            var deliveryDate = DateTime.Today.GetNextWorkingDay();

            bag.Logger.LogInformation($"Calling the FED API to get a product forecast for {deliveryDate} and supplier ID {(int)Suppliers.AbelAndCole}.");
            var supplierForecast = await bag.FedClient.GetSupplierForecastAsync((int)Suppliers.AbelAndCole, deliveryDate);

            var hasPantry = false;
            var hasChill = false;
            var hasFruitAndVeg = false;

            var deliveryDateForecast = supplierForecast?[deliveryDate];

            if (deliveryDateForecast is object && deliveryDateForecast.Count > 0)
            {
                bag.Logger.LogInformation("Calling the FED API to get all products.");
                var products = await bag.FedClient.GetProductsAsync();

                var chilledCategories = new List<string> { "Chilled Drinks", "Dairy", "Deli" };
                var fruitAndVegCategories = new List<string> { "Fruit & Veg" };
                var pantryCategories = new List<string> { "Alcohol", "Ambient Drinks", "Bakery", "Dairy Alts", "Eggs", "Packaged" };

                bag.Logger.LogInformation("Iterating through the product forecast and determining Abel & Cole's staffing requirements...");

                foreach (var productQuantity in deliveryDateForecast)
                {
                    var product = products.Single(p => !p.IsDeleted && p.IsSuppliedByAbelAndCole() && p.SupplierSKU == productQuantity.SupplierSKU);

                    if (chilledCategories.Contains(product.ProductGroup))
                        hasChill = true;
                    else if (pantryCategories.Contains(product.ProductGroup))
                        hasPantry = true;
                    else if (fruitAndVegCategories.Contains(product.ProductGroup))
                        hasFruitAndVeg = true;

                    if (hasChill && hasPantry && hasFruitAndVeg)
                        break;
                }
            }

            bag.Logger.LogInformation("Preparing Abel & Cole email text...");

            var html = await EmailTemplates.ApplyAbelAndColeStaffingTemplateAsync(deliveryDate, hasPantry, hasChill, hasFruitAndVeg);

            bag.Logger.LogInformation("Sending off email...");

            var email = new Email
            {
                FromAddress = "noreply@fedteam.co.uk",
                CCs = bag.Config.AbelAndColeStaffingCCEmailAddresses,
                ToAddresses = bag.Config.AbelAndColeStaffingToEmailAddresses,
                BCCs = bag.Config.FedOpsEmailAddresses,
                Subject = "Operations Forecast",
                PlainText = "",
                HtmlText = html.ToString()
            };

            var emailSent = await bag.SendGridService.SendMessageAsync(email);
        }
    }
}