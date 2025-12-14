using Fed.Web.Service.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
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
            Log.Logger =
                new LoggerConfiguration()
                    .MinimumLevel.Warning()
                    .Enrich.WithProperty("Application", "Fed.Web.Service")
                    .WriteTo.Console()
                    .CreateLogger();

            try
            {
                Log.Information("Starting Fed.Web.Service...");

                WebBuilderExtensions.UseAzureKeyVault(args)
                    .UseSerilog()
                    .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                        webBuilder.UseIISIntegration();
                        webBuilder.UseStartup<Startup>();
                    })
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
