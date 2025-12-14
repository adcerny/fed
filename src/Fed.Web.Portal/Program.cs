using Fed.Web.Portal.Extensions;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using System;
using System.IO;

namespace Fed.Web.Portal
{
    public class Program
    {
        public static int Main(string[] args) => StartWebServer(args);

        public static int StartWebServer(string[] args)
        {
            Log.Logger =
                new LoggerConfiguration()
                    .MinimumLevel.Warning()
                    .Enrich.WithProperty("Application", "Fed.Web.Portal")
                    .WriteTo.Console()
                    .CreateLogger();

            try
            {
                Log.Information("Starting Fed.Web.Portal...");

                WebBuilderExtensions.UseAzureKeyVault(args)
                    .UseSerilog()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
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
