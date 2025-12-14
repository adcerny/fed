using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Fed.Data.CustomerImport
{
    public class Program
    {
        public static int Main(string[] args) => Run(args);

        public static int Run(string[] args)
        {
            try
            {
                Console.WriteLine("Starting Fed Customer Import...");

                var rootDirectory = Directory.GetCurrentDirectory();
                var config =
                    new ConfigurationBuilder()
                        .SetBasePath(rootDirectory)
                        .AddJsonFile("settings.json", true, false)
                        .Build();

                var braintreeSection = config.GetSection("Braintree");

                var braintreeConfig = new BraintreeConfig
                {
                    Environment = braintreeSection["Environment"],
                    MerchantId = braintreeSection["MerchantId"],
                    MerchantAccountId = braintreeSection["MerchantAccountId"],
                    PublicKey = braintreeSection["PublicKey"],
                    PrivateKey = braintreeSection["PrivateKey"]
                };

                var dbName = config["DatabaseToUse"];
                var connectionString =
                    config.GetConnectionString(dbName)
                    ?? throw new InvalidOperationException("Cannot import customers without connection string.");

                var csvFileName =
                    args != null && args.Length == 1
                    ? args[0]
                    : Path.Combine(Directory.GetCurrentDirectory(), config["CSVFileName"]);

                Console.WriteLine($"Importing {csvFileName}...");
                var customerCount = CustomerImporter.ImportFromCsvAsync(csvFileName, connectionString, braintreeConfig).Result;
                Console.WriteLine($"Successfully imported {customerCount} customers.");

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
            if (args == null || args.Length == 0)
                Console.ReadLine();

            return code;
        }
    }
}