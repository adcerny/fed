using Fed.Api.External.AzureStorage;
using Fed.Api.External.IdealPostcodesService;
using Fed.Api.External.PostcodesIOService;
using Fed.Api.External.XeroService;
using Fed.Core.Common;
using Fed.Core.Common.Interfaces;
using Fed.Core.Data.Handlers;
using Fed.Core.Services;
using Fed.Core.Services.Factories;
using Fed.Core.Services.Interfaces;
using Fed.Infrastructure.Data.Handlers;
using Fed.Infrastructure.Data.SqlServer;
using Fed.Infrastructure.Data.SqlServer.Handlers;
using Fed.Infrastructure.Factories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xero.Api;

namespace Fed.Web.Service.Config
{
    public static class DependencyConfig
    {
        public static IServiceCollection AddFed(this IServiceCollection services, IConfiguration config, IHostEnvironment env)
        {
            // Load Configuration values:
            // ------------------------------

            string connectionString = null;

            if (env.IsDevelopment())
            {
                var activeDatabase = config.GetSection("ActiveDatabase").Value;
                connectionString = config.GetConnectionString(activeDatabase);
            }
            else
            {
                connectionString = config["connection-string"];
            }

            var azureStorageConnectionString = config["AzureStorageConnectionString"];
            var idealPostcodeServiceApiKey = config["ideal-postcode-service-api-key"];
            var braintreeEnvironment = config[$"braintree-environment"];
            var braintreeMerchantId = config[$"braintree-merchant-id"];
            var braintreeMerchantAccountId = config[$"braintree-merchant-account-id"];
            var braintreePublicKey = config[$"braintree-public-key"];
            var braintreePrivateKey = config[$"braintree-private-key"];
            var xeroCertificate = config[$"xero-certificate"];
            var xeroCertificatePassword = config[$"xero-certificate-password"];
            var xeroConsumerKey = config[$"xero-consumer-key"];
            var xeroConsumerSecret = config[$"xero-consumer-secret"];
            var paymentMaxConcurrentOperations = int.Parse(config["MaxConcurrentPaymentOperations"]);


            // Default .NET Dependencies:
            // ------------------------------
            services.AddHttpClient();
            services.AddLogging();
            services.AddTransient(svc => svc.GetService<ILoggerFactory>().CreateLogger("DefaultLogger"));

            // Fed.Core/Fed.Infrastructure dependencies:
            // ------------------------------

            // Configs:
            services.AddTransient<ISqlServerConfig>(svc => new SqlServerConfig(connectionString));
            services.AddTransient<IAzureConfig>(svc => new AzureConfig(azureStorageConnectionString));

            // Handlers:
            services.AddTransient<IReportingHandler, ReportingHandler>();
            services.AddTransient<IHolidaysHandler, HolidaysHandler>();
            services.AddTransient<IHubsHandler, HubsHandler>();
            services.AddTransient<IOrdersHandler, OrdersHandler>();
            services.AddTransient<IDeliveriesHandler, DeliveriesHandler>();
            services.AddTransient<IDeliveryShortageHandler, DeliveryShortageHandler>();
            services.AddTransient<IDeliveryAdditionHandler, DeliveryAdditionHandler>();
            services.AddTransient<IProductsHandler, ProductsHandler>();
            services.AddTransient<IRecurringOrdersHandler, RecurringOrdersHandler>();
            services.AddTransient<ISkipDatesHandler, SkipDatesHandler>();
            services.AddTransient<ITimeslotsHandler, TimeslotsHandler>();
            services.AddTransient<ICustomersHandler, CustomersHandler>();
            services.AddTransient<IDeliveryAddressHandler, DeliveryAddressHandler>();
            services.AddTransient<IBillingAddressHandler, BillingAddressHandler>();
            services.AddTransient<ICardTokenHandler, CardTokenHandler>();
            services.AddTransient<IContactsHandler, ContactsHandler>();
            services.AddTransient<IPaymentsHandler, PaymentRequestsHandler>();
            services.AddTransient<IInvoicesHandler, InvoicesHandler>();
            services.AddTransient<IDeliveryBoundaryHandler, DeliveryBoundaryHandler>();
            services.AddTransient<IPostcodeLocationHandler, PostcodeLocationHandler>();
            services.AddTransient<IDiscountsHandler, DiscountsHandler>();
            services.AddTransient<ISuppliersHandler, SuppliersHandler>();
            services.AddTransient<IHolidaysHandler, HolidaysHandler>();
            services.AddTransient<ICustomerAgentsHandler, CustomerAgentsHandler>();
            services.AddTransient<IForecastedOrdersHandler, ForecastedOrdersHandler>();
            services.AddTransient<ICustomerMarketingAttributesHandler, CustomerMarketingAttributesHandler>();
            services.AddTransient<IProductCategoriesHandler, ProductCategoriesHandler>();

            // Services:
            services.AddTransient<IOrderIdGenerator, OrderIdGenerator>();
            services.AddTransient<IDeliveryIdGenerator, DeliveryIdGenerator>();
            services.AddTransient<IIdGenerator, RandomIdGenerator>();
            services.AddTransient<IForecastService, ForecastService>();
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<IDeliveryService, DeliveryService>();
            services.AddTransient<ISuppliersService, SuppliersService>();
            services.AddTransient<IPostcodeHubService, PostcodeDeliverableService>();
            services.AddTransient<IDeliveryBoundaryService, DeliveryBoundaryService>();
            services.AddTransient<IInvoiceService, InvoiceService>();
            services.AddTransient<IBakeryMinimumOrderService, MinimumOrderService>();
            services.AddTransient<ICustomerService, CustomerService>();
            services.AddTransient<IAzureQueueService, AzureQueueService>();
            services.AddTransient<IDiscountService, DiscountService>();
            services.AddTransient<IHolidayService, HolidayService>();
            services.AddTransient<ICustomerAgentService, CustomerAgentService>();
            services.AddTransient<IXeroApiSettings, XeroApiSettings>();
            services.AddTransient<IDiscountStrategyFactory, DiscountStrategyFactory>();
            services.AddTransient<ICustomerMarketingAttributeService, CustomerMarketingAttributeService>();
            services.AddTransient<IProductCategoryService, ProductCategoryService>();
            services.AddTransient<ITimeslotsService, TimeslotsService>();
            services.AddTransient<IProductsService, ProductsService>();

            services.AddTransient<IRecurringOrderService>(
                svc =>
                    new RecurringOrderService(
                        svc.GetService<IRecurringOrdersHandler>(),
                        svc.GetService<ISkipDatesHandler>(),
                        svc.GetService<IForecastService>(),
                        svc.GetService<ICustomersHandler>(),
                        svc.GetService<ITimeslotsService>(),
                        svc.GetService<IDiscountService>())); ;

            services.AddTransient<IPostcodeAddressesService>(
                svc =>
                    new IdealPostcodesService(
                        idealPostcodeServiceApiKey,
                        null,
                        svc.GetService<IPostcodeHubService>()));

            services.AddTransient<IPostcodeLocationService>(
                svc =>
                    new IdealPostcodesService(
                        idealPostcodeServiceApiKey,
                        new PostcodesIOService(),
                        null));

            services.AddTransient<IPaymentGatewayService>(
                svc =>
                    PaymentProcessingFactory.CreateBraintreeGatewayService(
                        braintreeEnvironment,
                        braintreeMerchantId,
                        braintreeMerchantAccountId,
                        braintreePublicKey,
                        braintreePrivateKey,
                        connectionString,
                        svc.GetService<ILogger>()));

            services.AddTransient<IProcessPaymentsService>(
                svc =>
                    PaymentProcessingFactory.CreateProcessPaymentsService(
                        braintreeEnvironment,
                        braintreeMerchantId,
                        braintreeMerchantAccountId,
                        braintreePublicKey,
                        braintreePrivateKey,
                        connectionString,
                        paymentMaxConcurrentOperations,
                        svc.GetService<ILogger>()));

            services.AddTransient<IExternalInvoiceService>(
                svc =>
                    new XeroInvoiceService(
                        new XeroSettings(
                            xeroConsumerKey,
                            xeroConsumerSecret,
                            xeroCertificate,
                            xeroCertificatePassword),
                        svc.GetService<ILogger>()));

            return services;
        }
    }
}
