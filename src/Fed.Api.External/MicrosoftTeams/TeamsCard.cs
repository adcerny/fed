using Fed.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fed.Api.External.MicrosoftTeams
{
    public class TeamsCard
    {
        private TeamsCard()
        { }

        public CardType CardType { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Summary { get; set; }
        public IList<KeyValuePair<string, string>> Facts { get; set; }
        public IList<KeyValuePair<string, string>> Sections { get; set; }
        public IList<KeyValuePair<string, string>> UrlActions { get; set; }

        private static string JsonEncode(string str) => str.Replace(Environment.NewLine, "\n").Replace("\\", "\\\\").Replace("\"", "\\\"");

        private static IList<KeyValuePair<string, string>> GetDefaultFacts(string functionName) =>
            new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Timestamp", DateTime.Now.ToBritishTime().ToString("dddd, dd MMMM yyyy HH:mm:ss")),
                    new KeyValuePair<string, string>("Function", functionName)
                };

        public static TeamsCard Create(
            CardType cardType,
            string functionName,
            string title,
            string subTitle,
            string summary,
            List<KeyValuePair<string, string>> additionalFacts = null,
            List<KeyValuePair<string, string>> sections = null,
            List<KeyValuePair<string, string>> urlActions = null)
        {
            return new TeamsCard
            {
                CardType = cardType,
                Title = title,
                SubTitle = subTitle,
                Summary = summary,
                Facts = GetDefaultFacts(functionName).Union(additionalFacts ?? new List<KeyValuePair<string, string>>()).ToList(),
                Sections = sections,
                UrlActions = urlActions
            };
        }

        private static TeamsCard CreateException(
            CardType cardType,
            Exception ex,
            string functionName,
            string title = null)
        {
            var stackTrace = new StringBuilder();
            stackTrace.AppendLine("```");
            stackTrace.AppendLine(ex.StackTrace.Replace("\\", "\\\\"));
            stackTrace.AppendLine("```");

            return Create(
                cardType,
                functionName,
                title ?? "An unexpected error has occured",
                $"The Azure function with the name '{functionName}' has failed with an unhandled exception.",
                $"An exception of type '{ex.GetType()}' has been thrown.",
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Exception source", ex.Source),
                    new KeyValuePair<string, string>("Exception target site", ex.TargetSite.Name)
                },
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("Exception Message:", JsonEncode(ex.Message)),
                    new KeyValuePair<string, string>("Stack Trace:", stackTrace.ToString())
                },
                null);
        }

        public static TeamsCard CreateError(Exception ex, string functionName) =>
            CreateException(CardType.Error, ex, functionName);

        public static TeamsCard CreateWarning(Exception ex, string functionName, string title) =>
            CreateException(CardType.Warning, ex, functionName, title);

        public static TeamsCard CreateWarning(List<string> warnings, string functionName)
        {
            var summary =
                warnings.Count == 1
                ? "A total of 1 warning has been raised."
                : $"A total of {warnings.Count} warnings have been raised.";

            var warningMsg = new StringBuilder();

            foreach (var warning in warnings)
                warningMsg.AppendLine($"- {warning}");

            return
                Create(
                    CardType.Warning,
                    functionName,
                    "Attention: Some critical warnings have been raised by the system",
                    $"The Azure function with the name '{functionName}' has finished with warnings.",
                    summary,
                    null,
                    new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("Warnings:", warningMsg.ToString())
                    },
                    null);
        }

        private string GetThemeColour()
        {
            switch (CardType)
            {
                case CardType.Order: return "75bf4e";
                case CardType.Payment: return "9242f4";
                case CardType.Delivery: return "4274f4";
                case CardType.Error: return "f4424e";
                case CardType.Warning: return "f4bf42";
                case CardType.SupplierOrder: return "207ab2";
                default: return "666666";
            }
        }

        private string GetImageBaseUrl()
        {
            // Allow overriding the image base URL via environment variable to avoid exposing storage account names.
            var overrideUrl = Environment.GetEnvironmentVariable("TEAMS_CARD_IMAGE_BASE_URL");
            if (!string.IsNullOrWhiteSpace(overrideUrl))
                return overrideUrl.TrimEnd('/');

            return "https://fedstorageaccountprd.blob.core.windows.net/fed-bot";
        }

        private string GetCardImage()
        {
            var baseUrl = GetImageBaseUrl();
            switch (CardType)
            {
                case CardType.Order: return $"{baseUrl}/order.png";
                case CardType.Payment: return $"{baseUrl}/payment2.png";
                case CardType.Delivery: return $"{baseUrl}/delivery.png";
                case CardType.Error: return $"{baseUrl}/error.png";
                case CardType.Warning: return $"{baseUrl}/warning.png";
                case CardType.SupplierOrder: return $"{baseUrl}/supplierOrder.png";
                case CardType.Invoice: return $"{baseUrl}/invoice.png";
                default: return "";
            }
        }

        public string AsJson()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            sb.AppendLine("\"@type\": \"MessageCard\",");
            sb.AppendLine("\"@context\": \"http://schema.org/extensions\",");
            sb.AppendLine($"\"themeColor\": \"{GetThemeColour()}\",");
            sb.AppendLine($"\"summary\": \"{Title}\",");
            sb.AppendLine("\"sections\": [");

            // Activity Section
            sb.AppendLine("{");
            sb.AppendLine($"\"activityTitle\": \"{Title}\",");
            sb.AppendLine($"\"activitySubtitle\": \"{SubTitle}\",");
            sb.AppendLine($"\"activityText\": \"{Summary}\",");
            sb.AppendLine($"\"activityImage\": \"{GetCardImage()}\",");

            if (Facts != null && Facts.Count > 0)
            {
                sb.AppendLine("\"facts\": [");

                bool isFirst = true;

                foreach (var fact in Facts)
                {
                    if (!isFirst) sb.Append(",");

                    sb.AppendLine("{");
                    sb.AppendLine($"\"name\": \"{fact.Key}\",");
                    sb.AppendLine($"\"value\": \"{fact.Value}\"");
                    sb.AppendLine("}");

                    isFirst = false;
                }

                sb.Append("],");
            }

            sb.AppendLine("\"markdown\": true");
            sb.AppendLine("}");

            // Additional Sections
            if (Sections != null && Sections.Count > 0)
            {
                foreach (var section in Sections)
                {
                    sb.Append(",");
                    sb.AppendLine("{");
                    sb.AppendLine($"\"title\": \"{section.Key}\",");
                    sb.AppendLine($"\"text\": \"{section.Value}\"");
                    sb.AppendLine("}");
                }
            }

            sb.AppendLine("],");

            // Potential Actions
            sb.AppendLine("\"potentialAction\": [");

            if (UrlActions != null && UrlActions.Count > 0)
            {
                var isFirst = true;

                foreach (var action in UrlActions)
                {
                    if (!isFirst) sb.Append(",");

                    sb.AppendLine("{");
                    sb.AppendLine("\"@context\": \"http://schema.org\",");
                    sb.AppendLine("\"@type\": \"ViewAction\",");
                    sb.AppendLine($"\"name\": \"{action.Key}\",");
                    sb.AppendLine($"\"target\": [\"{action.Value}\"]");
                    sb.AppendLine("}");

                    isFirst = false;
                }
            }

            sb.AppendLine("]");

            // Finish:
            sb.Append("}");

            return sb.ToString();
        }
    }
}
