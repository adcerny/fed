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
    public class LabelsController : Controller
    {
        private readonly FedWebClient _fedWebClient;

        public LabelsController(FedWebClient fedWebClient)
        {
            _fedWebClient = fedWebClient;
        }

        [HttpGet("/labels/{timeslotId}")]
        public async Task<IActionResult> Index(Guid timeslotId)
        {
            var timeslotTask = _fedWebClient.GetTimeslotAsync(timeslotId);

            var vm = new LabelViewModel
            {
                CurrentTimeslot = await timeslotTask
            };
            
            return View(vm);
        }

        [HttpGet("/labels/{timeslotId}/model")]
        public async Task<IActionResult> GetLabelModel(Guid timeslotId)
        {
            var timeslotTask = _fedWebClient.GetTimeslotAsync(timeslotId);

            var vm = new LabelViewModel
            {
                CurrentTimeslot = await timeslotTask
            };

            return Json(vm);
        }
    }
}