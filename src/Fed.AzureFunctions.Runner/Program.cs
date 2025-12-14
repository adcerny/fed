using Fed.Api.External.XeroService;
using Fed.AzureFunctions.Core;
using Fed.AzureFunctions.Functions;
using Fed.Core.ValueTypes;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Fed.AzureFunctions.Runner
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                FunctionsEnvironment.LoadEnvironment();

                int userInput = 0;
                do
                {
                    userInput = DisplayMenu();

                    switch (userInput)
                    {
                        case 1:
                            RunPaymentProcessingWithParam().Wait();
                            break;
                        case 2:
                            FunctionRunner.RunWebAsync(new ConsoleLogger(), "RaiseInvoicesFunction", null, RaiseInvoicesFunction.RaiseInvoices).Wait();
                            break;
                        case 3:
                            FunctionRunner.RunWebAsync(new ConsoleLogger(), "ProductSyncFunction", null, ProductSyncFunction.ProductSync).Wait();
                            break;
                        case 4:
                            FunctionRunner.RunAsync(new ConsoleLogger(), "BakeryMinimumOrderPastryFunctionCreate", BakeryMinimumOrderPastryFunction.TopUpBakeryPastryOrder);
                            break;
                        case 5:
                            FunctionRunner.RunAsync(new ConsoleLogger(), "BakeryMinimumOrderPastryFunctionCreate", BakeryNotifyPastryOrdersFunction.NotifyPastryOrders);
                            break;
                        case 6:
                            PrepareXeroDemoAccounts().Wait();
                            break;
                        case 7:
                            FunctionRunner.RunAsync(new ConsoleLogger(), "SupplierForecastFunction", SupplierForecastFunction.PlaceAbelAndColeForecast);
                            break;
                        case 8:
                            FunctionRunner.RunAsync(new ConsoleLogger(), "SupplierForecastFunction", SupplierForecastFunction.PlaceAbelAndColeForecast);
                            break;
                        default:
                            Console.WriteLine("Please enter a valid option");
                            break;

                    }

                } while (userInput != 9);
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
                Console.WriteLine();
                Console.WriteLine();

                return Main(args);
            }

        }

        private static async Task RunPaymentProcessingWithParam()
        {
            Date date = ReadDate("Please enter a delivery date (hit enter for today):");

            await FunctionRunner.RunWithArgAsync(
                 new ConsoleLogger(), "PaymentProcessingFunction", date.ToString(), PaymentProcessingFunction.ProcessPayments);
        }

        private static int Exit(string[] args, int code)
        {
            if (args != null && args.Length > 0 && args[0] == "pauseonexit")
                Console.ReadLine();

            return code;
        }

        static public int DisplayMenu()
        {
            Console.WriteLine("Azure Functions");
            Console.WriteLine("---------------");
            Console.WriteLine("1. PaymentProcessingFunction");
            Console.WriteLine("2. RaiseInvoicesFunction");
            Console.WriteLine("3. ProductSyncFunction");
            Console.WriteLine("4. BakeryMinimumOrderPastryFunction");
            Console.WriteLine("5. BakeryNotifyPastryOrdersFunction");
            Console.WriteLine("6. Prepare demo accounts");
            Console.WriteLine("7. Supplier forecast function");
            Console.WriteLine("8. Exit");
            return ReadInt("Please select a function.");
        }

        private static int ReadInt(string prompt)
        {
            Console.WriteLine(prompt);
            while (true)
            {
                int result;

                if (int.TryParse(Console.ReadLine(), out result))
                    return result;

                Console.WriteLine("Please enter an valid integer.");
            }
        }

        private static DateTime ReadDate(string prompt)
        {

            Console.WriteLine(prompt);
            while (true)
            {
                DateTime result;

                var input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Using today's date");
                    return DateTime.Today;
                }

                if (DateTime.TryParse(input, out result))
                    return result;

                Console.WriteLine("Please enter a valid date (e.g. 2019-01-01)");
            }
        }

        private static async Task PrepareXeroDemoAccounts()
        {
            var prodSettings = FunctionsEnvironment.GetXeroSettings("prod");
            var demoSettings = FunctionsEnvironment.GetXeroSettings("dev");

            var prodService = new XeroAccountService(prodSettings, new ConsoleLogger());
            var demoService = new XeroAccountService(demoSettings, new ConsoleLogger());

            var accounts = prodService.GetXeroAccounts(new DateTime(2018, 1, 1));
            try
            {
                await demoService.CreateAccounts(accounts.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
