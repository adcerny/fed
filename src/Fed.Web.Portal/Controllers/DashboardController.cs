using CsvHelper;
using Fed.Api.External.AzureStorage;
using Fed.Core.Common;
using Fed.Core.Enums;
using Fed.Core.Extensions;
using Fed.Core.ValueTypes;
using Fed.Web.Portal.Extensions;
using Fed.Web.Portal.Models.Home;
using Fed.Web.Portal.Models.Reports;
using Fed.Web.Portal.ViewModels;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;
        private readonly IAzureTableService _tableService;

        public DashboardController(
            IConfiguration config,
            FedWebClient fedWebClient,
            IAzureTableService tableService)
        {
            _config = config;
            _fedWebClient = fedWebClient;
            _tableService = tableService;
        }

        [HttpGet("/dashboard/sales")]
        public async Task<IActionResult> Sales(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var viewModel =
                new SalesDashboardViewModel
                {
                    FedWebServiceUrl = _config["FED_WEB_SERVICE_URL"],
                    FromDate = fromDate ?? DateTime.Today.AddDays(7 * 4 * (0 - 1)),
                    ToDate = toDate ?? DateTime.Today
                };

            var nameAndQuery = $"GetSalesHistoryPerDay?fromDate={viewModel.FromDate.ToString("yyyy-MM-dd")}&toDate={viewModel.ToDate.ToString("yyyy-MM-dd")}";

            var report = await _fedWebClient.GetReportAsync(nameAndQuery);

            viewModel.ReportModel = new ReportModel(nameAndQuery, report, null, null);

            var salesSummaryReport = await _fedWebClient.GetReportAsync<AccountSalesStatistic>("SalesSummary");

            var today = DateTime.Today.ToBritishTime();
            var week1Number = today.AddWeeks(-3).ToIsoWeekOfYear();
            var week2Number = today.AddWeeks(-2).ToIsoWeekOfYear();
            var week3Number = today.AddWeeks(-1).ToIsoWeekOfYear();
            var week4Number = today.ToIsoWeekOfYear();

            var salesSummary = new SalesSummary
            {
                Week1Name = $"Week {week1Number}",
                Week2Name = $"Week {week2Number}",
                Week3Name = $"Week {week3Number}",
                Week4Name = $"Week {week4Number}",
                SalesStats = new List<AccountTypeStat>()
            };

            var groupedSalesReport = salesSummaryReport.GroupBy(x => x.AccountType);

            foreach (var group in groupedSalesReport)
            {
                var accountTypeStat =
                    new AccountTypeStat
                    {
                        AccountType = group.Key,
                        Week1 = new WeeklyStat(),
                        Week2 = new WeeklyStat(),
                        Week3 = new WeeklyStat(),
                        Week4 = new WeeklyStat()
                    };

                foreach (var item in group)
                {
                    if (item.Week.Equals(week1Number))
                    {
                        accountTypeStat.Week1.Deliveries = item.Deliveries;
                        accountTypeStat.Week1.Sales = item.Sales;
                    }

                    if (item.Week.Equals(week2Number))
                    {
                        accountTypeStat.Week2.Deliveries = item.Deliveries;
                        accountTypeStat.Week2.Sales = item.Sales;
                    }

                    if (item.Week.Equals(week3Number))
                    {
                        accountTypeStat.Week3.Deliveries = item.Deliveries;
                        accountTypeStat.Week3.Sales = item.Sales;
                    }

                    if (item.Week.Equals(week4Number))
                    {
                        accountTypeStat.Week4.Deliveries = item.Deliveries;
                        accountTypeStat.Week4.Sales = item.Sales;
                    }
                }

                salesSummary.SalesStats.Add(accountTypeStat);
            }

            viewModel.SalesSummary = salesSummary;

            return View("Sales", viewModel);
        }

        [HttpGet("/dashboard/customers")]
        public async Task<IActionResult> Customers(string lifecycleStatus = null, int? accountTypeId = null)
        {
            var customers = await _fedWebClient.GetCustomersAsync(true);

            var queryArgs = new Dictionary<string, string>
            {
                { "LifecycleStatus", lifecycleStatus },
                { "AccountTypeId", accountTypeId.HasValue ? accountTypeId.Value.ToString() : "" }
            };

            var query = QueryString.Create(queryArgs);

            var reportNameAndQuery = $"GetCustomers{query}";

            var report = await _fedWebClient.GetReportAsync(reportNameAndQuery);

            var viewModel =
                new CustomersDashboardViewModel
                {
                    FedWebServiceUrl = _config["FED_WEB_SERVICE_URL"],
                    Customers = customers,
                    LifecycleStatus = lifecycleStatus,
                    AccountTypeId = accountTypeId,
                    ReportModel =
                        new ReportModel(
                            reportNameAndQuery,
                            report,
                            new[]
                            {
                                new ReportModel.FieldLink(
                                    "Company ID",
                                    "Company ID",
                                    "@Id",
                                    "customers/@Id")
                            },
                            new[] { "CustomerId" }
                        )
                };

            return View("Customers", viewModel);
        }

        [HttpGet("/dashboard/suppliers")]
        public async Task<IActionResult> SuppliersAsync()
        {
            var model = await _fedWebClient.GetSuppliersAsync();

            return View("Suppliers", model);

        }

        [HttpGet("/dashboard/ops")]
        public async Task<IActionResult> Operations()
        {
            var deliveries = await _fedWebClient.GetDeliveriesAsync(Date.Today);

            if (deliveries != null && deliveries.Count > 0)
            {
                var pickOrder = await _tableService.GetCurrentPickOrderAsync();
                var delivery =
                    deliveries.SortDeliveries(pickOrder).FirstOrDefault(d => d.BagCount == 0)
                    ?? deliveries.First();
                ViewBag.NextDeliveryId = delivery.Id;
            }

            ViewBag.DeliveryForecastUrl = $"{_config["FED_SUPPLIER_BASE_URL"]}/deliveryForecast";

            return View("Operations");
        }

        [HttpGet("/dashboard/system")]
        public async Task<IActionResult> System()
        {
            var serviceInfo = await _fedWebClient.GetInfoAsync();
            var customerAgents = await _fedWebClient.GetCustomerAgentsAsync();
            var customerMarketingAttributes = await _fedWebClient.GetCustomerMarketingAttributesAsync();
            var productCategories = await _fedWebClient.GetProductCategoriesAsync();

            var viewModel = new SystemDashboardViewModel
            {
                ServiceInfo = serviceInfo,
                PortalInfo = PortalInfo.GetInfo(),
                ProductSyncUrl = _config["PRODUCT_SYNC_URL"],
                BrainTreeMigrationUrl = _config["BRAINTREE_MIGRATION_URL"],
                CustomerAgents = customerAgents?.OrderBy(o => o.Name).ToList(),
                CustomerMarketingAttributes = customerMarketingAttributes?.OrderBy(o => o.Name).ToList(),
                ProductCategories = productCategories?.OrderBy(p=>p.Name).ToList()
            };

            return View("System", viewModel);
        }

        [HttpGet("/")]
        public async Task<IActionResult> Home()
        {
            var newCustomersReportName = "NewCustomerSummary";
            var accountsCreatedReportName = "AccountsCreatedReport";
            var newCustomersReportTask = _fedWebClient.GetReportAsync(newCustomersReportName);
            var accountsCreatedReportTask = _fedWebClient.GetReportAsync(accountsCreatedReportName);
            var accountsCreatedTask = GetAccountsCreated();

            var viewModel =
                new HomeDashboardViewModel
                {
                    FedWebServiceUrl = _config["FED_WEB_SERVICE_URL"],
                    CustomerMarketingAttributes = await _fedWebClient.GetCustomerMarketingAttributesAsync(),
                    NewCustomerSummary =
                        new ReportModel(
                            newCustomersReportName,
                            await newCustomersReportTask,
                            new[]
                            {
                                new ReportModel.FieldLink(
                                    "CompanyName",
                                    "Id",
                                    "@Id",
                                    "customers/@Id")
                            },
                            new[] { "Id" }),
                    AccountsCreatedReport =
                        new ReportModel(
                            accountsCreatedReportName,
                            await accountsCreatedReportTask,
                            new[]
                            {
                                new ReportModel.FieldLink(
                                    "Company Name",
                                    "CustomerId",
                                    "@Id",
                                    "customers/@Id")
                            },
                            new[] { "CustomerId" }),
                    AccountsCreated = await accountsCreatedTask
                };

            return View("Home", viewModel);
        }

        [HttpGet("/dashboard/download/accountsCreated")]
        public async Task<IActionResult> DownloadAccountsCreated(string reportName)
        {
            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.CultureInfo = CultureInfo.GetCultureInfo("en-GB");

                var data = await GetAccountsCreatedForDownload();

                csv.WriteRecords(data);
                var contents = Encoding.UTF8.GetBytes(writer.ToString());

                return File(contents, "text/csv", $"AccountsCreated-{DateTime.Now.ToString("yyyy-MM-dd")}.csv");
            }

        }

        private async Task<IList<AccountsCreatedViewModel>> GetAccountsCreated()
        {
            
            
            var customersTask = _fedWebClient.GetCustomersAsync(true);
            var forecastTask = _fedWebClient.GetForecastCustomerIds(DateTime.Today, DateTime.Today.AddWeeks(8));
            var earliestDate = DateTime.Today.EquivalentWeekDay(DayOfWeek.Monday).AddDays(-14);

            var customers = await customersTask;

            var newCustomers = customers.Where(c => c.RegisterDate >= earliestDate && c.AccountType != AccountType.Deleted)
                                        .OrderByDescending(c => c.RegisterDate);

            var forecast = await forecastTask;

            List<AccountsCreatedViewModel> vm = new List<AccountsCreatedViewModel>();
            foreach (var customer in newCustomers)
            {
                Date? nextDeliveryDate = null;
                foreach (var date in forecast.Keys)
                {
                    if (forecast[date].Any(id => id == customer.Id))
                    {
                        nextDeliveryDate = date;
                        break;
                    }
                }
                vm.Add(new AccountsCreatedViewModel(customer, nextDeliveryDate));
            }
            return vm;
        }

        private async Task<IList<AccountsCreatedDownloadViewModel>> GetAccountsCreatedForDownload()
        {
            var customersTask = _fedWebClient.GetCustomersAsync(true);
            var forecastTask = _fedWebClient.GetForecastCustomerIds(DateTime.Today, DateTime.Today.AddWeeks(8));
            var earliestDate = DateTime.Today.EquivalentWeekDay(DayOfWeek.Monday).AddWeeks(-8);

            var customers = await customersTask;

            var newCustomers = customers.Where(c => c.RegisterDate >= earliestDate && c.AccountType != AccountType.Deleted)
                                        .OrderByDescending(c => c.RegisterDate);

            var forecast = await forecastTask;

            List<AccountsCreatedDownloadViewModel> vm = new List<AccountsCreatedDownloadViewModel>();
            foreach (var customer in newCustomers)
            {
                Date? nextDeliveryDate = null;
                foreach (var date in forecast.Keys)
                {
                    if (forecast[date].Any(id => id == customer.Id))
                    {
                        nextDeliveryDate = date;
                        break;
                    }
                }
                vm.Add(new AccountsCreatedDownloadViewModel(customer, nextDeliveryDate));
            }
            return vm;
        }
    }
}
