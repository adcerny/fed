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
                Console.WriteLine("Starting Fed Products Import...");

                var rootDirectory = Directory.GetCurrentDirectory();
                var config =
                    new ConfigurationBuilder()
                        .SetBasePath(rootDirectory)
                        .AddJsonFile("settings.json", true, false)
                        .Build();

                var dbName = config["DatabaseToUse"];
                var connectionString =
                    config.GetConnectionString(dbName)
                    ?? throw new InvalidOperationException("Cannot import products without connection string.");

                var csvFileName = config["CSVFileName"];

                Console.WriteLine($"Importing {csvFileName}...");
                var productCount = ProductsImporter.ImportFromCsvAsync(csvFileName, connectionString).Result;
                Console.WriteLine($"Successfully imported {productCount} products.");

                return Exit(args, 0);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("An unhandeled exception has been thrown:");
                Console.WriteLine("Message: {0}", ex.Message);
                Console.WriteLine("Source: {0}", ex.Source);
                Console.WriteLine("StackTrace: {0}", ex.StackTrace);
                Console.ResetColor();

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
