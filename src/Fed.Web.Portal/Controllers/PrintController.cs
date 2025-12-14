using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fed.Web.Portal.ViewModels;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Fed.Web.Portal.Controllers
{
    public class PrintController : Controller
    {
        private readonly FedWebClient _fedWebClient;

        public PrintController(FedWebClient fedWebClient)
        {
            _fedWebClient = fedWebClient;
        }

        [HttpGet("/print/{timeslotId}")]
        public async Task<IActionResult> Index(Guid timeslotId)
        {
            var timeslotTask = _fedWebClient.GetTimeslotAsync(timeslotId);

            var vm = new PrintViewModel
            {
                Timeslot = await timeslotTask
            };
            
            return View(vm);
        }
    }
}