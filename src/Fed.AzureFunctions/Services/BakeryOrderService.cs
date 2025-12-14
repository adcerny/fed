using Fed.Api.External.MicrosoftTeams;
using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Entities;
using Fed.Core.Entities;
using Fed.Core.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Services
{
    public static class BakeryOrderService
    {
        public static async Task SendProductOrdersAsync(
            string functionName,
            ServicesBag bag,
            DateTime deliveryDate,
            List<Product> productsToSend = null)
        {
            var fedClient = bag.FedClient;
            var fedBot = bag.FedBot;
            var logger = bag.Logger;

            // 1. Retrieve all Seven Seeded products:
            // -------------------------------

            var orderedProducts = await fedClient.GetSupplierConfirmedOrdersAsync((int)Suppliers.SevenSeeded, deliveryDate);

            if (productsToSend != null)
                orderedProducts = orderedProducts.Where(o => productsToSend.Any(p => p.ProductCode.Equals(o.ProductCode))).ToList();


            if (orderedProducts == null || orderedProducts.Count == 0)
            {
                logger.LogInformation($"No products needed to be ordered from the bakery for the date {deliveryDate}.");

                await fedBot.SendMessage(
                    TeamsCard.Create(
                        CardType.SupplierOrder,
                        functionName,
                        $"No products had to be ordered for supplier Seven Seeded.",
                        $"No Fed orders require bakery orders from Seven Seeded for the date {deliveryDate.ToString("dddd, dd MMMM yyyy")}.",
                        string.Empty,
                        null,
                        null,
                        null));

                return;
            }

            // 2. Populate Email template:
            // -------------------------------

            var productRows = new List<(string, string, int)>();

            foreach (var pq in orderedProducts)
            {
                productRows.Add((pq.SupplierSKU, pq.ProductName, pq.SupplierQuantity));
            }

            var html = await EmailTemplates.ApplySevenSeededTemplateAsync(deliveryDate, productRows);

            // 3. Send Email:
            // -------------------------------

            var title = $"Orders for {deliveryDate.ToString("dddd, dd MMMM yyyy")}";

            var email = new Email
            {
                FromAddress = "noreply@fedteam.co.uk",
                ToAddresses = bag.Config.SevenSeededEmailAddresses,
                CCs = bag.Config.FedBuyersEmailAddresses,
                BCCs = bag.Config.FedOpsEmailAddresses,
                Subject = title,
                PlainText = "",
                HtmlText = html.ToString()
            };

            var result = await bag.SendGridService.SendMessageAsync(email);

            // 4. Notify in Teams:
            // -------------------------------
            var subTitle =
                productRows.Count == 1
                ? $"A total of 1 product has been ordered."
                : $"A total of {productRows.Count} products have been ordered.";

            var urlActions = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(
                    $"View Orders for {deliveryDate.ToString("dd/MM/yyyy")}",
                    $"{bag.Config.FedPortalUrl}/supplier/7?deliveryDate={deliveryDate.ToString("yyyy-MM-dd")}")
            };

            await fedBot.SendMessage(
                TeamsCard.Create(
                    CardType.SupplierOrder,
                    functionName,
                    $"Product orders have been placed for supplier Seven Seeded.",
                    subTitle,
                    string.Empty,
                    new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Delivery Date", deliveryDate.ToString("dddd, dd MMMM yyyy")) },
                    null,
                    urlActions));
        }
    }
}
