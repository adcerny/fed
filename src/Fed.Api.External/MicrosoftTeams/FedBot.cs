using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Api.External.MicrosoftTeams
{
    public class FedBot
    {
        private readonly ILogger _logger;
        private readonly string _webHookUrl;
        private readonly HttpClient _client;

        public FedBot(
            ILogger logger,
            HttpClient client,
            string webHookUrl)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _webHookUrl = webHookUrl ?? throw new ArgumentNullException(nameof(webHookUrl));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public static FedBot Create(ILogger logger, string webHookUrl) => new FedBot(logger, new HttpClient(), webHookUrl);

        public async Task SendMessage(TeamsCard card)
        {
            try
            {
                var json = card.AsJson();
                var response =
                    await _client.PostAsync(
                        _webHookUrl,
                        new StringContent(json, Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Couldn't send message to Microsoft Teams. The request failed with status code {code} and message {msg}.", response.StatusCode, responseContent);

                    _logger.LogInformation("Request JSON payload:");
                    _logger.LogInformation(json);
                }

                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}