using Fed.Web.Service.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using System.IO;

namespace Fed.Web.Service
{
    public class Program
    {
        public static int Main(string[] args) => StartWebServer(args);

        public static int StartWebServer(string[] args)
        {
            var loggerConfig =
                new LoggerConfiguration()
                    .MinimumLevel.Warning()
                    .Enrich.WithProperty("Application", "Fed.Web.Service")
                    .WriteTo.ApplicationInsightsTraces(TelemetryConfiguration.Active)
                    .WriteTo.Console();

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                Log.Information("Starting Fed.Web.Service...");
                WebBuilderExtensions.UseAzureKeyVault(args)
                .UseSerilog()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build()
                .Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
                return -1;
            }
            finally
            {
                Log.CloseAndFlush();
                Console.ReadLine();
            }
        }
    }
}
