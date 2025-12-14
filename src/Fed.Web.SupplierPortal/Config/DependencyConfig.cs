using Fed.Web.Service.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fed.Web.SupplierPortal.Config
{
    public static class DependencyConfig
    {
        public static IServiceCollection AddFed(this IServiceCollection services, IConfiguration config)
        {
            services.AddHttpClient();
            services.AddLogging();
            services.AddTransient(svc => svc.GetService<ILoggerFactory>().CreateLogger("DefaultLogger"));

            services.AddTransient(
                svc => FedWebClient.Create(
                    svc.GetService<ILogger>(),
                    baseUrl: config.GetValue<string>("FED_WEB_SERVICE_URL")));

            return services;
        }
    }
}