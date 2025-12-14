using Fed.Core.Data.Handlers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class ReportingController : ControllerBase
    {
        private readonly IReportingHandler _reportingHandler;

        public ReportingController(IReportingHandler reportingHandler)
        {
            _reportingHandler = reportingHandler;
        }

        [HttpGet("/reports/{reportName}")]
        public async Task<ActionResult<IList<object>>> GetReportAsync(string reportName)
        {
            dynamic obj = new ExpandoObject();

            var query = Request.Query;

            if (query != null && query.Keys != null && query.Keys.Count > 0)
            {
                foreach (var key in query.Keys)
                {
                    var value = query[key].ToString();
                    ((IDictionary<string, object>)obj).Add(key, value);
                }
            }

            var report = await _reportingHandler.GetReportAsync<object>(reportName, (object)obj);

            return Ok(report);
        }

        [HttpGet("/reports/NewCustomerSummary")]
        public async Task<ActionResult<IList<object>>> GetNewCustomerSummaryAsync()
        {
            var report = await _reportingHandler.GetNewCustomerSummaryAsync<object>();

            return Ok(report);
        }        
    }
}