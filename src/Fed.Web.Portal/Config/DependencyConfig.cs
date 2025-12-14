using Fed.Api.External.ActivityLogs;
using Fed.Api.External.AzureStorage;
using Fed.Api.External.FreshSalesService;
using Fed.Api.External.MerchelloService;
using Fed.Core.Common;
using Fed.Core.Common.Interfaces;
using Fed.Core.Services.Interfaces;
using Fed.Label.V2;
using Fed.Web.Service.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace Fed.Web.Portal.Config
{
    public static class DependencyConfig
    {
        public static IServiceCollection AddFed(this IServiceCollection services, IConfiguration config)
        {
            var azureStorageConnectionString = config["AZURE_STORAGE_CONNECTION_STRING"];

            var freshsalesApiKey = config[$"freshsales-api-key"];

            services.AddHttpClient();
            services.AddLogging();
            services.AddTransient(svc => svc.GetService<ILoggerFactory>().CreateLogger("DefaultLogger"));

            services.AddTransient(
                svc => FedWebClient.Create(
                    svc.GetService<ILogger>(),
                    baseUrl: config.GetValue<string>("FED_WEB_SERVICE_URL")));

            services.AddTransient(
                svc => MerchelloAPIClient.Create(
                    new HttpClient(),
                    config.GetValue<string>("FED_MERCHELLO_BASE_URL"),
                    config.GetValue<string>("FED_MERCHELLO_SECRET_TOKEN"),
                    null));

            services.AddTransient(
                svc => LogonActivityClient.Create(
                    config.GetValue<string>("CUSTOMER_ACTIVITY_URL")));


            services.AddTransient<IAzureConfig>(svc => new AzureConfig(azureStorageConnectionString));
            services.AddTransient<ICRMService>(svc => new FreshSalesService(freshsalesApiKey));
            services.AddTransient<IAzureTableService, AzureTableService>();
            services.AddTransient<IPrintLabelService, PrintLabelService>();

            return services;
        }
    }
}
