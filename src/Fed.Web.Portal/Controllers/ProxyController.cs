using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class ProxyController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;

        public ProxyController(IConfiguration config, FedWebClient fedWebClient)
        {
            _config = config;
            _fedWebClient = fedWebClient;
        }

        [HttpGet("/proxy/reports/{reportName}")]
        public async Task<IActionResult> Report(string reportName)
        {
            var reportNameAndQuery = $"{reportName}{Request.QueryString.ToString()}";
            var report = await _fedWebClient.GetReportAsync(reportNameAndQuery);
            return Ok(report);
        }
    }
}