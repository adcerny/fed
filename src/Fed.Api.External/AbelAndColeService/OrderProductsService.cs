using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Api.External.AbelAndColeService
{
    public class OrderProductsService
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public OrderProductsService(
            ILogger logger,
            HttpClient httpClient,
            string baseUrl)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = new Uri(baseUrl);
        }

        public static OrderProductsService Create(
            ILogger logger,
            HttpClient client = null,
            string baseUrl = "")
            => new OrderProductsService(logger, client ?? new HttpClient(), baseUrl);

        private static int GetCustomerIdByDate(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday: return 1209871;
                case DayOfWeek.Tuesday: return 1209872;
                case DayOfWeek.Wednesday: return 1209873;
                case DayOfWeek.Thursday: return 1209874;
                case DayOfWeek.Friday: return 1209875;
                default:
                    throw new InvalidOperationException("Abel & Cole does not deliver on weekends.");
            }
        }

        public async Task<List<ProductQuantity>> TryGetOrderAsync(
            DateTime deliveryDate)
        {
            var customerId = GetCustomerIdByDate(deliveryDate);
            var request = new HttpRequestMessage(
                HttpMethod.Get, $"/v1/customer/{customerId}/orders/{deliveryDate.ToString("yyyy-MM-dd")}/items");

             var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            try
            {
                response.EnsureSuccessStatusCode();
                var result = JsonConvert.DeserializeObject<List<ProductQuantity>>(responseContent);
                return (result);
            }
            catch (HttpRequestException)
            {
                var errorMessage = $"HTTP Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}, Message: {responseContent}";

                _logger.LogError("Order for '{requestUri}' didn't finish successfully.", request.RequestUri);
                _logger.LogError("Abel & Cole API failed with status code: {statusCode}", response.StatusCode);
                _logger.LogError("Abel & Cole API failed with message: {message}", responseContent);
                return new List<ProductQuantity>();
            }
            catch (Exception ex)
            {
                _logger.LogError("An unhandled exception occurred: {error}", ex.Message);
                return new List<ProductQuantity>();
            }

        }

            public async Task<(int, string)> TrySendOrderAsync(
            DateTime deliveryDate,
            ProductQuantity productQuantity,
            int depth = 0)
        {
            var customerId = GetCustomerIdByDate(deliveryDate);

            var request = new HttpRequestMessage(
                HttpMethod.Put, $"/v1/customer/{customerId}/order/{deliveryDate.ToString("yyyy-MM-dd")}/item");

            var json = JsonConvert.SerializeObject(productQuantity);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            // ToDo: Check with A&C guys why we put this here again.
            // Either way, it must mean that the desired quantity
            // couldn't be placed, which is why we return 0:
            if (response.StatusCode == HttpStatusCode.NoContent)
                return (0, null);

            var responseContent = await response.Content.ReadAsStringAsync();
            try
            {
                // Conflict means that the quantity which we requested couldn't
                // be placed entirely:
                if (response.StatusCode == HttpStatusCode.Conflict)
                {
                    int qauntityPlaced = 0;
                    string errorMessage = null;

                    _logger.LogWarning("Order for '{requestUri}' didn't finish successfully.", request.RequestUri);
                    _logger.LogWarning("Abel & Cole API failed with status code: {statusCode}", response.StatusCode);
                    _logger.LogWarning("Abel & Cole API failed with message: {message}", responseContent);

                    var failedResult = JsonConvert.DeserializeObject<FailedResponse>(responseContent);

                    if (failedResult.QuantityAvailable > 0 && depth < 3)
                    {
                        var pq = new ProductQuantity
                        {
                            ProductId = productQuantity.ProductId,
                            Quantity = failedResult.QuantityAvailable
                        };
                        (qauntityPlaced, errorMessage) = await TrySendOrderAsync(deliveryDate, pq, depth++);
                    }

                    return (qauntityPlaced, errorMessage);
                }

                response.EnsureSuccessStatusCode();

                var result = JsonConvert.DeserializeObject<SucessfulResponse>(responseContent);
                return (result.Quantity, null);
            }
            catch (HttpRequestException)
            {
                var errorMessage = $"HTTP Status Code: {response.StatusCode}, Reason: {response.ReasonPhrase}, Message: {responseContent}";

                _logger.LogError("Order for '{requestUri}' didn't finish successfully.", request.RequestUri);
                _logger.LogError("Abel & Cole API failed with status code: {statusCode}", response.StatusCode);
                _logger.LogError("Abel & Cole API failed with message: {message}", responseContent);

                return (0, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError("An unhandled exception occured: {error}", ex.Message);
                return (0, ex.Message);
            }
        }
    }
}
