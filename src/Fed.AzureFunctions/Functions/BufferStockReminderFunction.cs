using Fed.Api.External.SendGridService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Entities;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Functions
{
    public static class BufferStockReminderFunction
    {
        private const string FuncName = "BufferStockReminderFunction";
        private const string Schedule = "0 0 15 * * 1-5";

        [FunctionName(FuncName)]
        public static Task Run(
            [TimerTrigger(Schedule)]TimerInfo timerInfo,
            ILogger logger)
            => FunctionRunner.RunAsync(logger, FuncName, NotifyFedOpsTeam);

        public static async Task NotifyFedOpsTeam(ServicesBag bag)
        {
            bag.Logger.LogInformation("Sending reminder email...");

            var email = new Email
            {
                FromAddress = "noreply@fedteam.co.uk",
                ToAddresses = bag.Config.FedBufferStockEmailAddresses,
                Subject = "Buffer Stock Reminder",
                PlainText = "Hey Keith, just a quick reminder... did you do the buffer stock yet? If not please complete this before the deadline today! Thank you, Your Concierge",
                HtmlText = ""
            };

            var emailSent = await bag.SendGridService.SendMessageAsync(email);
        }
    }
}