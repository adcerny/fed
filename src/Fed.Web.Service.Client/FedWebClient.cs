using Fed.Core.Converters;
using Fed.Core.Entities;
using Fed.Core.Models;
using Fed.Core.ValueTypes;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Web.Service.Client
{
    public class FedWebClient
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public FedWebClient(
            ILogger logger,
            HttpClient httpClient,
            string baseUrl)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _httpClient.BaseAddress = new Uri(baseUrl);

            JsonConvert.DefaultSettings = () =>
                {
                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new DateJsonConverter());
                    return settings;
                };
        }

        public static FedWebClient Create(
            ILogger logger,
            HttpClient client = null,
            string baseUrl = "https://localhost:5001")
            => new FedWebClient(logger, client ?? new HttpClient(), baseUrl);

        // -----------------------------
        // Private Helper Methods
        // -----------------------------

        private async Task<TResponse> SendAsync<TResponse>(HttpMethod verb, string path, string jsonContent = null)
        {
            var request = new HttpRequestMessage(verb, path);

            if (jsonContent != null)
                request.Content =
                    new StringContent(
                        jsonContent,
                        Encoding.UTF8,
                        "application/json");

            var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Fed Web Service request failed with status code: {statusCode}.", response.StatusCode);
                _logger.LogError("Fed Web Service request failed with message: {message}.", responseContent);

                throw new HttpRequestException($"{(int)response.StatusCode} ({response.ReasonPhrase}): {responseContent}");
            }

            return typeof(TResponse) == typeof(string)
                ? (TResponse)(object)responseContent
                : JsonConvert.DeserializeObject<TResponse>(responseContent);
        }

        private Task PatchAsync(string path, params PatchOperation[] patches)
        {
            var content = new StringBuilder("[ ");

            for (var i = 0; i < patches.Length; i++)
            {
                content.Append(patches[i].ToString());

                if (i < patches.Length - 1)
                    content.Append(", ");
            }
            content.Append(" ]");

            return SendAsync<string>(new HttpMethod("PATCH"), path, content.ToString());
        }

        private Task<T> SendAsync<T>(HttpMethod verb, string path, object obj) => SendAsync<T>(verb, path, JsonConvert.SerializeObject(obj));
        private Task<T> GetAsync<T>(string path) => SendAsync<T>(HttpMethod.Get, path);
        private Task<T> PostAsync<T>(string path, object obj) => SendAsync<T>(HttpMethod.Post, path, obj);
        private Task<T> PutAsync<T>(string path, object obj) => SendAsync<T>(HttpMethod.Put, path, obj);
        private Task<T> PutAsync<T>(string path, string jsonContent) => SendAsync<T>(HttpMethod.Put, path, jsonContent);
        private Task<T> PutAsync<T>(string path) => SendAsync<T>(HttpMethod.Put, path);
        private Task<T> DeleteAsync<T>(string path) => SendAsync<T>(HttpMethod.Delete, path);

        // -----------------------------
        // Home
        // -----------------------------

        public Task<AppInfo> GetInfoAsync() => GetAsync<AppInfo>("/info");
        public Task<string> PingAsync() => GetAsync<string>("/ping");

        // -----------------------------
        // Hubs
        // -----------------------------

        public Task<IList<Hub>> GetHubsAsync() => GetAsync<IList<Hub>>("/hubs");

        public Task<Hub> UpdateHubAsync(Hub hub) => SendAsync<Hub>(HttpMethod.Put, $"/hubs/{hub.Id}", hub);

        // -----------------------------
        // Postcodes
        // -----------------------------

        public Task<string> GetPostcodeHubAsync(string postcode) => GetAsync<string>($"/postcodes/{postcode}/hubId");

        // -----------------------------
        // Timeslots
        // -----------------------------

        public Task<IList<Timeslot>> GetTimeslotsAsync(Guid hubId) => GetAsync<IList<Timeslot>>($"/timeslots?hubId={hubId}");

        public Task<Timeslot> GetTimeslotAsync(Guid Id) => GetAsync<Timeslot>($"/timeslots/{Id}");

        // -----------------------------
        // Products
        // -----------------------------

        public Task<IList<Product>> GetProductsAsync(Guid? productGroup = null, Guid? productCategoryId = null)
        {
            var queryParams = new Dictionary<string, string>();
            if (productGroup.HasValue)
                queryParams.Add("productGroup", productGroup.ToString());
            if (productCategoryId.HasValue)
                queryParams.Add("productCategoryId", productCategoryId.ToString());

            return GetAsync<IList<Product>>(QueryHelpers.AddQueryString("/products", queryParams));
        }

        public Task<Product> GetProductAsync(string id) => GetAsync<Product>($"/products/{id}");

        public Task<Product> CreateProductAsync(Product product) => PostAsync<Product>($"/products/", product);

        public Task<bool> DeleteProductAsync(string id) => DeleteAsync<bool>($"/products/{id}");

        public Task PatchProductAsync(string id, params PatchOperation[] patches) =>
            PatchAsync($"/products/{id}", patches);

        // -----------------------------
        // Customers
        // -----------------------------

        public Task<Customer> CreateCustomerAsync(Customer model) => PostAsync<Customer>("/customers", model);

        public Task<Guid> CreateDeliveryAddressAsync(Guid customerId, Guid contactId, DeliveryAddress da) => PostAsync<Guid>($"/customers/{customerId}/contacts/{contactId}/deliveryAddresses", da);

        public Task<Guid> CreateBillingAddressAsync(Guid customerId, Guid contactId, BillingAddress ba) => PostAsync<Guid>($"/customers/{customerId}/contacts/{contactId}/billingAddress", ba);

        public Task<IList<Customer>> GetCustomersAsync(bool includeContacts) => GetAsync<IList<Customer>>($"/customers?includeContacts={includeContacts}");

        public Task<Customer> GetCustomerAsync(Guid customerId) => GetAsync<Customer>($"/customers/{customerId}");

        public Task<FullCustomerInfo> GetCustomerFullInfoAsync(string customerId) => GetAsync<FullCustomerInfo>($"/customers/{customerId}/fullInfo");

        public Task PatchCustomerAsync(Guid id, params PatchOperation[] patches) => PatchAsync($"/customers/{id}", patches);

        public Task PatchContactAsync(Guid customerId, Guid contactId, params PatchOperation[] patches) =>
            PatchAsync($"/customers/{customerId}/contacts/{contactId}", patches);

        public Task PatchDeliveryAddressAsync(Guid customerId, Guid contactId, Guid deliveryAddressId, params PatchOperation[] patches) =>
            PatchAsync($"/customers/{customerId}/contacts/{contactId}/deliveryAddresses/{deliveryAddressId}", patches);

        public Task PatchBillingAddressAsync(Guid customerId, Guid contactId, Guid billingAddressId, params PatchOperation[] patches) =>
            PatchAsync($"/customers/{customerId}/contacts/{contactId}/billingAddress/{billingAddressId}", patches);

        public Task PatchCardTokenAsync(Guid customerId, Guid contactId, Guid cardTokenId, params PatchOperation[] patches) =>
            PatchAsync($"/customers/{customerId}/contacts/{contactId}/cardTokens/{cardTokenId}", patches);

        public Task<CardToken> PostCardTokenAsync(Guid customerId, Guid contactId, CardPaymentRequest command) =>
            PostAsync<CardToken>($"/customers/{customerId}/contacts/{contactId}/cardTokens", command);

        public Task<bool> DeleteCardTokenAsync(Guid customerId, Guid contactId, Guid cardTokenId) =>
            DeleteAsync<bool>($"/customers/{customerId}/contacts/{contactId}/cardTokens/{cardTokenId}");

        public async Task<string> GetClientTokenAsync() => 
            JsonConvert.DeserializeObject<string>(await GetAsync<string>($"/customers/cardTokens/clientToken"));

        public Task UpdateMarketingConsentAsync(string emailAddress, bool isMarketingConsented) => PutAsync<bool>($"/marketingConsent/{emailAddress}", isMarketingConsented);

        public Task<IList<CustomerAgent>> GetCustomerAgentsAsync() => GetAsync<IList<CustomerAgent>>($"/CustomerAgents");

        public Task<CustomerAgent> CreateCustomerAgentAsync(CustomerAgent customerAgent) => PostAsync<CustomerAgent>($"/CustomerAgents/", customerAgent);

        public Task<bool> DeleteCustomerAgentAsync(Guid customerAgentId) => DeleteAsync<bool>($"/CustomerAgents/{customerAgentId}");

        // -----------------------------
        // Recurring Orders
        // -----------------------------

        public Task<IList<RecurringOrder>> GetRecurringOrdersAsync(Guid contactId, Date fromDate, Date toDate) =>
            GetAsync<IList<RecurringOrder>>($"/recurringorders?contactId={contactId}&fromDate={fromDate}&toDate={toDate}");

        public Task<IList<RecurringOrder>> GetRecurringOrdersAsync(Date fromDate, Date toDate) =>
            GetAsync<IList<RecurringOrder>>($"/recurringorders?fromDate={fromDate}&toDate={toDate}");

        public Task<RecurringOrder> GetRecurringOrderAsync(Guid id) => GetAsync<RecurringOrder>($"/recurringorders/{id}");

        public Task<RecurringOrder> CreateRecurringOrderAsync(RecurringOrder model) => PostAsync<RecurringOrder>("/recurringorders", model);

        public Task<RecurringOrder> PostRecurringOrderFromDate(Guid id, Date date, RecurringOrder model) => PostAsync<RecurringOrder>($"/recurringorders/{id}/{date}", model);

        public Task<RecurringOrder> PutRecurringOrderForSingleDate(Guid id, Date date, RecurringOrder model) => PutAsync<RecurringOrder>($"/recurringorders/{id}/{date}", model);

        public Task<RecurringOrder> UpdateRecurringOrderAsync(Guid id, RecurringOrder model) => PutAsync<RecurringOrder>($"/recurringorders/{id}", model);

        public Task PatchRecurringOrderAsync(Guid id, params PatchOperation[] patches) => PatchAsync($"/recurringorders/{id}", patches);

        public Task<bool> DeleteRecurringOrderAsync(Guid recurringOrderId) => DeleteAsync<bool>($"/recurringorders/{recurringOrderId}");

        public Task<OrderDeliveryContext<RecurringOrder>> GetRecurringOrderForDate(Guid recurringOrderId, Date date) => GetAsync<OrderDeliveryContext<RecurringOrder>>($"/recurringorders/{recurringOrderId}/{date}");

        // -----------------------------
        // Skip Dates
        // -----------------------------

        public Task<IList<SkipDate>> GetSkipDatesAsync(Guid recurringOrderId) =>
            GetAsync<IList<SkipDate>>($"/recurringorders/{recurringOrderId}/skipdates");

        public Task<IList<SkipDate>> SetSkipDateAsync(Guid recurringOrderId, Date date, string reason, string createdBy) =>
            PutAsync<IList<SkipDate>>(
                $"/recurringorders/{recurringOrderId}/skipdates/{date}",
                $"{{ \"reason\": \"{reason}\", \"createdBy\": \"{createdBy}\" }}");

        public Task<IList<SkipDate>> DeleteSkipDateAsync(Guid recurringOrderId, Date date) => DeleteAsync<IList<SkipDate>>($"/recurringorders/{recurringOrderId}/skipdates/{date}");

        // -----------------------------
        // Forecast
        // -----------------------------

        public async Task<IDictionary<Date, IList<RecurringOrder>>> GetForecastAsync(Date fromDate, Date toDate, Guid? contactId = null)
        {
            var url = $"/forecast?fromDate={fromDate}&toDate={toDate}";
            if (contactId.HasValue)
                url += $"&contactId={contactId.Value}";

            var reuslt = await GetAsync<IDictionary<DateTime, IList<RecurringOrder>>>(url);

            var forecast = new SortedDictionary<Date, IList<RecurringOrder>>();

            foreach (var kv in reuslt)
                forecast.Add(Date.Create(kv.Key), kv.Value);

            return forecast;
        }

        // -----------------------------
        // Forecast (Recurring Order Ids Only)
        // -----------------------------

        public async Task<IDictionary<Date, IList<Guid>>> GetForecastRecurringOrderIds(Date fromDate, Date toDate, Guid? contactId = null)
        {
            var url = $"/forecast/recurringOrderIds?fromDate={fromDate}&toDate={toDate}";
            if (contactId.HasValue)
                url += $"&contactId={contactId.Value}";

            var reuslt = await GetAsync<IDictionary<DateTime, IList<Guid>>>(url);

            var forecast = new SortedDictionary<Date, IList<Guid>>();

            foreach (var kv in reuslt)
                forecast.Add(Date.Create(kv.Key), kv.Value);

            return forecast;
        }

        // -----------------------------
        // Forecast (Customer Ids Only)
        // -----------------------------
        public async Task<IDictionary<Date, IList<Guid>>> GetForecastCustomerIds(Date fromDate, Date toDate)
        {
            var url = $"/forecast/customerIds?fromDate={fromDate}&toDate={toDate}";

            var reuslt = await GetAsync<IDictionary<DateTime, IList<Guid>>>(url);

            var forecast = new SortedDictionary<Date, IList<Guid>>();

            foreach (var kv in reuslt)
                forecast.Add(Date.Create(kv.Key), kv.Value);

            return forecast;
        }

        // -----------------------------
        // Delivery Forecast
        // -----------------------------
        public async Task<IDictionary<DateTime, IList<ForecastedDeliveries>>> GetForecastDeliveriesAsync(Date fromDate, Date toDate) =>
            await GetAsync<IDictionary<DateTime, IList<ForecastedDeliveries>>>($"/forecast/deliveries?fromDate={fromDate}&toDate={toDate}");

        public Task<IDictionary<DateTime, IList<SupplierProductQuantity>>> GetSupplierForecastAsync(int supplier, Date toDate) =>
            GetAsync<IDictionary<DateTime, IList<SupplierProductQuantity>>>($"/suppliers/{(int)supplier}/forecast?toDate={toDate}");

        // -----------------------------
        // Orders
        // -----------------------------

        public Task<IList<Order>> GetOrdersAsync(Date date) => GetAsync<IList<Order>>($"/orders?date={date}");

        public Task<Order> GetOrderAsync(Guid id) => GetAsync<Order>($"/orders/{id.ToString()}");

        public Task<IList<GeneratedOrder>> PlaceOrdersAsync(Date date) => PutAsync<IList<GeneratedOrder>>($"/orders/{date}");

        public Task<IList<SupplierProductQuantity>> GetSupplierConfirmedOrdersAsync(int supplierId, Date date) =>
            GetAsync<IList<SupplierProductQuantity>>($"/suppliers/{supplierId}/{date}/products");

        // -----------------------------
        // Deliveries
        // -----------------------------

        public Task<IList<Delivery>> CreateDeliveriesAsync(Date date) => PutAsync<IList<Delivery>>($"/deliveries/{date}");

        public Task<IList<Delivery>> GetDeliveriesAsync(Date date) => GetAsync<IList<Delivery>>($"/deliveries?date={date}");

        public Task<Delivery> GetDeliveryAsync(string id) => GetAsync<Delivery>($"/deliveries/{id}");

        public Task<bool> DeleteDeliveriesAsync(Date date) => DeleteAsync<bool>($"/deliveries/{date}");

        public Task<Delivery> SetDeliveryPackagingStatusAsync(Guid deliveryId, int packagingStatusId) =>
            PutAsync<Delivery>($"/deliveries/{deliveryId}/packagingStatus/{packagingStatusId}");

        public Task<Delivery> SetDeliveryBagCountAsync(string deliveryId, int bagCount) =>
            PutAsync<Delivery>($"/deliveries/{deliveryId}/bagCount/{bagCount}");

        // -----------------------------
        // Delivery Shortages
        // -----------------------------

        public Task<DeliveryShortage> ShortDeliveryItemAsync(DeliveryShortage deliveryShortage) =>
            PostAsync<DeliveryShortage>("/deliveryShortages", deliveryShortage);

        public Task<IList<DeliveryShortage>> GetDeliveryShortagesAsync(Date date) => GetAsync<IList<DeliveryShortage>>($"/deliveryShortages/{date}");

        public Task<string> DeleteDeliveryShortageAsync(Guid deliveryShortageId) => DeleteAsync<string>($"/deliveryShortages/{deliveryShortageId}");

        // -----------------------------
        // Delivery Additions
        // -----------------------------

        public Task<IList<DeliveryAddition>> GetDeliveryAdditionsAsync(Date date) => GetAsync<IList<DeliveryAddition>>($"/deliveryAdditions/{date}");

        public Task<DeliveryAddition> CreateDeliveryAddition(string deliveryId, DeliveryAddition deliveryAddition) =>
            PostAsync<DeliveryAddition>("/deliveryAdditions", deliveryAddition);

        public Task<string> DeleteDeliveryAdditionAsync(Guid deliveryAdditionId) => DeleteAsync<string>($"/deliveryAdditions/{deliveryAdditionId}");

        // -----------------------------
        // Invoices
        // -----------------------------

        public Task<List<Invoice>> GetInvoicesRequiredAsync(Date fromDate, Date toDate) => GetAsync<List<Invoice>>($"/invoices/required/?fromDate={fromDate.ToString()}&toDate={toDate.ToString()}");

        public Task<Guid> PostInvoiceAsync(Invoice invoice) => PostAsync<Guid>($"/invoices", invoice);

        // -----------------------------
        // Reports
        // -----------------------------

        public Task<IList<JObject>> GetReportAsync(string reportName) => GetAsync<IList<JObject>>($"/reports/{reportName}");

        public Task<IList<T>> GetReportAsync<T>(string reportName) => GetAsync<IList<T>>($"/reports/{reportName}");

        // -----------------------------
        // Discounts
        // -----------------------------

        public Task<Discount> CreateDiscountAsync(Discount discount) => PostAsync<Discount>($"/discounts", discount);

        public Task<Discount> UpdateDiscountAsync(Guid discountId, Discount discount) => PutAsync<Discount>($"/discounts/{discountId}", discount);

        public Task<bool> ApplyDiscountAsync(Guid discountId, Guid customerId) => PutAsync<bool>($"/discounts/{discountId}/{customerId}");

        public Task<IList<Discount>> GetDiscountsAsync() => GetAsync<IList<Discount>>($"/discounts");

        public Task<Discount> GetDiscountAsync(Guid id) => GetAsync<Discount>($"/discounts/{id}");

        public Task<IList<Discount>> GetDiscountsForCustomerAsync(Guid customerId) => GetAsync<IList<Discount>>($"/discounts?customerId={customerId}");

        public Task PatchDiscountAsync(Guid id, params PatchOperation[] patches) => PatchAsync($"/discounts/{id}", patches);

        // -----------------------------
        // Suppliers
        // -----------------------------
        public Task<List<Supplier>> GetSuppliersAsync() => GetAsync<List<Supplier>>($"/suppliers");

        public Task<Supplier> GetSupplierAsync(int id) => GetAsync<Supplier>($"/suppliers/{id}");

        public Task<Supplier> CreateSupplierAsync(Supplier model) => PostAsync<Supplier>("/suppliers", model);

        public Task PatchSupplierAsync(int id, params PatchOperation[] patches) => PatchAsync($"/suppliers/{id}", patches);

        // -----------------------------
        // Holidays
        // -----------------------------
        public Task<List<Holiday>> GetHolidaysAsync(Date fromDate, Date toDate) => GetAsync<List<Holiday>>($"/holidays?startDate={fromDate}&endDate={toDate}");

        public Task<bool> CreateHolidayAsync(Holiday holiday) => PostAsync<bool>("/holidays", holiday);

        public Task<bool> DeleteHolidayAsync(Date holidayDate) => DeleteAsync<bool>($"/holidays/{holidayDate}");

        // -----------------------------
        // Customer Marketing Attribute
        // -----------------------------
        public Task<List<CustomerMarketingAttribute>> GetCustomerMarketingAttributesAsync() => GetAsync<List<CustomerMarketingAttribute>>($"/customerMarketingAttributes");

        public Task<CustomerMarketingAttribute> GetCustomerMarketingAttributeAsync(Guid id) => GetAsync<CustomerMarketingAttribute>($"/customerMarketingAttributes/{id}");

        public Task<CustomerMarketingAttribute> CreateCustomerMarketingAttributeAsync(CustomerMarketingAttribute model) => PostAsync<CustomerMarketingAttribute>("/customerMarketingAttributes", model);

        public Task PatchCustomerMarketingAttributeAsync(Guid id, params PatchOperation[] patches) => PatchAsync($"/customerMarketingAttributes/{id}", patches);

        // -----------------------------
        // Product Categories
        // -----------------------------
        public Task<List<ProductCategory>> GetProductCategoriesAsync() => GetAsync<List<ProductCategory>>($"/ProductCategories");

        public Task<ProductCategory> GetProductCategoryAsync(Guid id) => GetAsync<ProductCategory>($"/ProductCategories/{id}");

        public Task<ProductCategory> CreateProductCategoryAsync(ProductCategory model) => PostAsync<ProductCategory>("/ProductCategories", model);

        public Task PatchProductCategoryAsync(Guid id, params PatchOperation[] patches) => PatchAsync($"/ProductCategories/{id}", patches);


    }
}