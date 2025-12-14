using Dapper;
using Fed.Core.Common;
using Fed.Core.Data.Commands;
using Fed.Core.Entities;
using Fed.Infrastructure.Data.SqlServer;
using Fed.Infrastructure.Data.SqlServer.Handlers;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fed.Data.Migration
{
    public class ProductsImporter
    {
        private static Product ParseCsvRow(IDictionary<string, int> headers, string[] values)
        {
            decimal price = 0m;

            if (decimal.TryParse(values[headers["Price"]], out decimal p))
                price = p;

            return new Product
            {
                Id = Guid.NewGuid().ToString(),
                ProductCode = "ToDo",
                ProductGroup = "ToDo",
                ProductName = values[headers["Product Description"]],
                SupplierId = values[headers["SupplierID"]] ?? "<Missing-In-Spreadsheet>",
                SupplierSKU = values[headers["SupplierSKU"]],
                Price = price,
                SalePrice = null,
                IsDeleted = false,
                DeletedDate = null,
                IsTaxable = true,
                IconCategory = ""
            };
        }

        private static IList<Product> ParseProducts(string csvFileName) => CsvParser.ParseCsv($"CSVs\\{csvFileName}", ParseCsvRow);

        public static async Task<int> ImportFromCsvAsync(string csvFileName, string connectionString)
        {
            var products = ParseProducts(csvFileName);

            var productsHandler = new ProductsHandler(new SqlServerConfig(connectionString));

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                connection.Execute("DELETE FROM [dbo].[Products]");
            }

            foreach (var product in products)
            {
                if (string.IsNullOrEmpty(product.SupplierSKU))
                {
                    Console.WriteLine($"Skipping the import of product '{product.ProductName}', because it doesn't have a valid SupplierSKU.");
                    continue;
                }

                try
                {
                    var createCmd = new CreateCommand<Product>(product);
                    await productsHandler.ExecuteAsync(createCmd);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR: An error occured when inserting a product into the database.");
                    Console.WriteLine($"Error Message: {ex.Message}");
                    Console.WriteLine($"Product: {product.ProductName}");
                }
            }

            return products.Count;
        }
    }
}
