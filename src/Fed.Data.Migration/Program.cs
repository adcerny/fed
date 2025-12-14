using DbUp;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Fed.Data.Migration
{
    public class Program
    {
        public static int Main(string[] args) => Run(args);

        public static int Run(string[] args)
        {
            try
            {
                Console.WriteLine("Starting Fed Database Migrations...");

                var rootDirectory = Directory.GetCurrentDirectory();
                var config =
                    new ConfigurationBuilder()
                        .SetBasePath(rootDirectory)
                        .AddJsonFile("settings.json", true, false)
                        .Build();

                var dbToUpgrade = config["DatabaseToUpgrade"];
                var connectionString =
                    config.GetConnectionString(dbToUpgrade)
                    ?? throw new InvalidOperationException("Cannot upgrade database without connection string.");

                var scriptsPath = Path.Combine(rootDirectory, "SQLScripts");

                var result =
                    DeployChanges.To
                        .SqlDatabase(connectionString)
                        .WithScriptsFromFileSystem(scriptsPath)
                        .LogToConsole()
                        .Build()
                        .PerformUpgrade();

                if (!result.Successful)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(result.Error);
                    Console.ResetColor();

                    Console.WriteLine();
                    Console.WriteLine("Press any key to exit");

                    Console.ReadKey();

                    return Exit(args, -1);
                }
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                return Exit(args, 0);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An unhandled exception has been thrown:");
                Console.WriteLine("Message: {0}", ex.Message);
                Console.WriteLine("Source: {0}", ex.Source);
                Console.WriteLine("StackTrace: {0}", ex.StackTrace);
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Press any key to exit");

                Console.ReadKey();

                return Exit(args, -1);
            }
        }

        private static int Exit(string[] args, int code)
        {
            if (args != null && args.Length > 0 && args[0] == "pauseonexit")
                Console.ReadLine();

            return code;
        }
    }
}