using Fed.Core.Exceptions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Fed.Api.External.MerchelloService
{
    public class MerchelloAPIClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly string _secretToken;

        public MerchelloAPIClient(HttpClient httpClient, string baseUrl, string secretToken, ILogger logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("token", secretToken);
            _logger = logger;
            _secretToken = secretToken;
        }

        public static MerchelloAPIClient Create(
            HttpClient client = null,
            string baseUrl = "",
            string secretToken = "",
            ILogger logger = null)
            => new MerchelloAPIClient(client ?? new HttpClient(), baseUrl, secretToken, logger);

        public async Task<IList<Product>> GetProductsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/products");
                var json = await response.Content.ReadAsStringAsync();
                var products = JsonConvert.DeserializeObject<IList<Product>>(json);
                return products;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An exception has been thrown when querying the Merchello API: {ex.Message}");
                return null;
            }
        }

        public async Task CreateUserAsync(User user)
        {
            var requestJson = JsonConvert.SerializeObject(user);
            var response =
                await _httpClient.PostAsync(
                    "/api/user",
                    new StringContent(
                        requestJson,
                        System.Text.Encoding.UTF8,
                        "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new DuplicateEmailAddresssException(user.EmailAddress, Guid.Empty);
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            var requestJson = JsonConvert.SerializeObject(user);
            var response =
                await _httpClient.PostAsync(
                    "/api/user/update",
                    new StringContent(
                        requestJson,
                        System.Text.Encoding.UTF8,
                        "application/json"));
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                throw new DuplicateEmailAddresssException(user.EmailAddress, Guid.Empty);
            }
            else
            {
                response.EnsureSuccessStatusCode();
            }
        }

        public async Task ForcePasswordResetEmail(string emailAddress)
        {
            var obj = new ResetPasswordRequest { EmailAddress = emailAddress };
            var requestJson = JsonConvert.SerializeObject(obj);
            var response =
                await _httpClient.PostAsync(
                    "/api/user/force-password-reset",
                    new StringContent(
                        requestJson,
                        System.Text.Encoding.UTF8,
                        "application/json"));
            response.EnsureSuccessStatusCode();
        }
    }
}