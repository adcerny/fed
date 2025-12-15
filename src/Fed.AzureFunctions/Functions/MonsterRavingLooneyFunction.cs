using Fed.Api.External.MicrosoftTeams;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class MonsterRavingLooneyFunction
    {
        private const string FuncName = "MonsterRavingLooneyFunction";
        private const string Schedule = "0 20 7,11,14,16,18 * * 1-5";

        public class Insanity
        {
            public string Type { get; set; }
            public string Message { get; set; }
        }

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timerInfo,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, ReportInsanityAsync);

        public static async Task ReportInsanityAsync(ServicesBag bag)
        {
            const int messageLimit = 30;

            var logger = bag.Logger;
            logger.LogInformation("Running the sanity report to detect any irregularities...");

            var insanities = await bag.FedClient.GetReportAsync<Insanity>("SanityReport");

            if (insanities == null || insanities.Count == 0)
            {
                logger.LogInformation($"No insanities detected. System looks suspiciously good...");
                logger.LogInformation("Hmmm... this is too good to be true, I'll better check later again...");
                return;
            }

            logger.LogInformation($"AHAAAAA!!! {insanities.Count} insanities found! WTF %^$&*£^$£^&*£$&*^!(&!!!");

            foreach (var insanity in insanities.Take(messageLimit))
                logger.LogInformation($"{insanity.Type}: {insanity.Message}");

            var teamsReport =
                insanities
                    .Select(i => new KeyValuePair<string, string>(i.Type, i.Message))
                    .OrderBy(kv => kv.Key)
                    .Take(messageLimit)
                    .ToList();

            var teamsCard =
                TeamsCard.Create(
                    CardType.Error,
                    FuncName,
                    "Chaos found!",
                    $"{insanities.Count} insanities detected.",
                    $"A total of {insanities.Count} issues have been found. Due to the maximum message capacity only {messageLimit} issues can be shown in Teams. If the total issue count exceeded the limit then please resolve those issues first and afterwards trigger the {FuncName} again.",
                    sections: teamsReport,
                    urlActions: new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("View Chaos", "https://portal.fedteam.co.uk/reports/SanityReport") });

            logger.LogInformation(teamsCard.AsJson());

            var webhookUrl = bag.Config?.TeamsWebhookUrl;
            if (string.IsNullOrWhiteSpace(webhookUrl))
            {
                logger.LogWarning("Microsoft Teams webhook URL is not configured. Skipping Teams notification.");
                return;
            }

            var chaosMonkey = FedBot.Create(bag.Logger, webhookUrl);

            logger.LogInformation("Sending insanity report to teams... need help from the devs, this ain't good, innit!");

            await chaosMonkey.SendMessage(teamsCard);
        }
    }
}