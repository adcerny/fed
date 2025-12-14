using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Api.External.SendGridService
{
    public static class EmailTemplates
    {
        private static string GetResourceName(string templateName) =>
            $"Fed.Api.External.SendGridService.Templates.{templateName}";

        private static Stream GetManifestStream(string templateName) =>
            typeof(EmailTemplates).GetTypeInfo().Assembly.GetManifestResourceStream(GetResourceName(templateName));

        public static async Task<string> ApplyAbelAndColeStaffingTemplateAsync(
            DateTime deliveryDate,
            bool hasPantry,
            bool hasChill,
            bool hasFandV)
        {
            using (var stream = GetManifestStream("AbelAndColeStaffing.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var template = await reader.ReadToEndAsync();

                    template = template.Replace("#DeliveryDate#", deliveryDate.ToString("dddd, dd MMMM yyyy"));
                    template = template.Replace("#HasPantry#", hasPantry ? "Yes" : "No");
                    template = template.Replace("#HasChill#", hasChill ? "Yes" : "No");
                    template = template.Replace("#HasFandV#", hasFandV ? "Yes" : "No");

                    return template;
                }
            }
        }

        public static async Task<string> ApplyZedifyNoOrdersTemplateAsync(
            DateTime deliveryDate)
        {
            using (var stream = GetManifestStream("ZedifyNoOrders.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var template = await reader.ReadToEndAsync();

                    template = template.Replace("#DeliveryDate#", deliveryDate.ToString("dddd, dd MMMM yyyy"));

                    return template;
                }
            }
        }

        public static async Task<string> ApplyZedifyTemplateAsync(
            DateTime deliveryDate,
            string csvLink)
        {
            using (var stream = GetManifestStream("Zedify.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var template = await reader.ReadToEndAsync();

                    template = template.Replace("#DeliveryDate#", deliveryDate.ToString("dddd, dd MMMM yyyy"));
                    template = template.Replace("#CSVLink#", csvLink);

                    return template;
                }
            }
        }

        public static async Task<string> ApplySevenSeededNotificationTemplateAsync(
            IList<(DateTime, string, string, int)> forecast)
        {
            var forecastRows = new StringBuilder();

            var even = false;

            foreach (var (date, sku, name, qty) in forecast)
            {
                var tr = even ? "<tr class=\"evenRow\">" : "<tr>";
                even = !even;

                forecastRows.AppendLine(tr);
                forecastRows.AppendLine($"<td class=\"right\">{date.ToString("ddd, dd/MM/yyyy")}</td>");
                forecastRows.AppendLine($"<td class=\"left\">{WebUtility.HtmlEncode(sku)}</td>");
                forecastRows.AppendLine($"<td class=\"left\">{WebUtility.HtmlEncode(name)}</td>");
                forecastRows.AppendLine($"<td class=\"right\">{qty}</td>");
                forecastRows.AppendLine($"</tr>");
            }

            using (var stream = GetManifestStream("SevenSeededNotification.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var template = await reader.ReadToEndAsync();

                    template = template.Replace("#ForecastRows#", forecastRows.ToString());

                    return template;
                }
            }
        }

        public static async Task<string> ApplySevenSeededPastryNotificationTemplateAsync(
            IList<(DateTime, string, string, int)> forecast,
            DateTime deliveryDate)
        {
            var forecastRows = new StringBuilder();

            var even = false;

            foreach (var (date, sku, name, qty) in forecast)
            {
                var tr = even ? "<tr class=\"evenRow\">" : "<tr>";
                even = !even;

                forecastRows.AppendLine(tr);
                forecastRows.AppendLine($"<td class=\"right\">{date.ToString("ddd, dd/MM/yyyy")}</td>");
                forecastRows.AppendLine($"<td class=\"left\">{WebUtility.HtmlEncode(sku)}</td>");
                forecastRows.AppendLine($"<td class=\"left\">{WebUtility.HtmlEncode(name)}</td>");
                forecastRows.AppendLine($"<td class=\"right\">{qty}</td>");
                forecastRows.AppendLine($"</tr>");
            }

            using (var stream = GetManifestStream("SevenSeededPastryNotification.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var template = await reader.ReadToEndAsync();

                    template = template.Replace("#ForecastRows#", forecastRows.ToString());
                    template = template.Replace("#DeliveryDate#", deliveryDate.ToString("ddd, dd/MM/yyyy"));

                    return template;
                }
            }
        }

        public static async Task<string> ApplySevenSeededTemplateAsync(
            DateTime deliveryDate,
            IList<(string, string, int)> productSkusNamesQuantities)
        {
            var productRows = new StringBuilder();

            var even = false;

            foreach (var (sku, name, qty) in productSkusNamesQuantities)
            {
                var tr = even ? "<tr class=\"evenRow\">" : "<tr>";
                even = !even;

                productRows.AppendLine(tr);
                productRows.AppendLine($"<td class=\"left\">{WebUtility.HtmlEncode(sku)}</td>");
                productRows.AppendLine($"<td class=\"left\">{WebUtility.HtmlEncode(name)}</td>");
                productRows.AppendLine($"<td class=\"right\">{qty}</td>");
                productRows.AppendLine($"</tr>");
            }

            using (var stream = GetManifestStream("SevenSeeded.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var template = await reader.ReadToEndAsync();

                    template = template.Replace("#DeliveryDate#", deliveryDate.ToString("dddd, dd MMMM yyyy"));
                    template = template.Replace("#ProductRows#", productRows.ToString());

                    return template;
                }
            }
        }

        public static async Task<string> ApplySupplierNotificationTemplateAsync(
            IList<(DateTime, string, string, int)> forecast,
            DateTime deliveryDate,
            string forecastUrl)
        {
            var forecastRows = new StringBuilder();

            var even = false;

            foreach (var (date, sku, name, qty) in forecast)
            {
                var tr = even ? "<tr class=\"evenRow\">" : "<tr>";
                even = !even;

                forecastRows.AppendLine(tr);
                forecastRows.AppendLine($"<td class=\"right\">{date.ToString("ddd, dd/MM/yyyy")}</td>");
                forecastRows.AppendLine($"<td class=\"left\">{WebUtility.HtmlEncode(sku)}</td>");
                forecastRows.AppendLine($"<td class=\"left\">{WebUtility.HtmlEncode(name)}</td>");
                forecastRows.AppendLine($"<td class=\"right\">{qty}</td>");
                forecastRows.AppendLine($"</tr>");
            }

            using (var stream = GetManifestStream("SupplierPurchaseOrder.html"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var template = await reader.ReadToEndAsync();

                    template = template.Replace("#ForecastRows#", forecastRows.ToString());
                    template = template.Replace("#DeliveryDate#", deliveryDate.ToString("ddd, dd/MM/yyyy"));
                    template = template.Replace("#ForecastUrl#", forecastUrl);

                    return template;
                }
            }
        }
    }
}
