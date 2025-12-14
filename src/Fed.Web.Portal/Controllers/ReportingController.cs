using CsvHelper;
using Fed.Web.Portal.Models.Reports;
using Fed.Web.Portal.ViewModels;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class ReportingController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;

        public ReportingController(IConfiguration config, FedWebClient fedWebClient)
        {
            _config = config;
            _fedWebClient = fedWebClient;
        }

        private async Task<ReportingViewModel> GetReportingViewModelAsync(string reportName, string query)
        {
            var reportNameAndQuery = $"{reportName}{query}";
            var report = await _fedWebClient.GetReportAsync(reportNameAndQuery);

           var title =
                reportName
                    .Split('-', StringSplitOptions.RemoveEmptyEntries)
                    .Aggregate((acc, next) => acc + " " + next);

            var model = new ReportModel(reportNameAndQuery, report, null, null);
            var viewModel = new ReportingViewModel(title, model);

            return viewModel;
        }

        [HttpGet("/reports/{reportName}")]
        public async Task<IActionResult> Report(string reportName)
        {
            var viewModel = await GetReportingViewModelAsync(reportName, Request.QueryString.ToString());
            viewModel.ReportModel.DisplayInputFieldsForParameters = true;
            return View("Report", viewModel);
        }

        [HttpGet("/reports/SupplierProductByDelivery")]
        public async Task<IActionResult> SupplierProductByDelivery()
        {
            var viewModel = await GetReportingViewModelAsync("SupplierProductByDelivery", Request.QueryString.ToString());
            viewModel.ReportModel.DisplayInputFieldsForParameters = true;
            return View("SupplierProductByDelivery", viewModel);
        }

        [HttpGet("/reports/download/{reportName}")]
        public async Task<IActionResult> DownloadReport(string reportName)
        {
            return await GetReportAsync(reportName);                   
        }

        private async Task<FileContentResult> GetReportAsync(string reportName)
        {
            var reportNameAndQuery = $"{reportName}{Request.QueryString.ToString()}";
            var report = await _fedWebClient.GetReportAsync(reportNameAndQuery);

            var records = new List<dynamic>();

            var exp = new ExpandoObjectConverter();
            foreach (var row in report)
                records.Add(JsonConvert.DeserializeObject<ExpandoObject>(row.ToString(), exp));

            using (var writer = new StringWriter())
            using (var csv = new CsvWriter(writer))
            {
                csv.Configuration.CultureInfo = CultureInfo.GetCultureInfo("en-GB");

                csv.WriteRecords(records);
                var contents = Encoding.UTF8.GetBytes(writer.ToString());

                return File(contents, "text/csv", $"{reportName}-{DateTime.Now.ToString("yyyy-MM-dd")}.csv");
            }
        }

    }
}
