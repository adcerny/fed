using Fed.Core.ValueTypes;
using Fed.Web.Portal.ViewModels;
using Fed.Web.Service.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Fed.Web.Portal.Controllers
{
    public class HolidaysController : Controller
    {
        private readonly IConfiguration _config;
        private readonly FedWebClient _fedWebClient;

        public HolidaysController(IConfiguration config, FedWebClient fedWebClient)
        {
            _config = config;
            _fedWebClient = fedWebClient;
        }


        [HttpPost("/holidays/SetHoliday")]
        public async Task<IActionResult> SetHoliday(PublicHolidayViewModel viewModel)
        {
            try
            {
                var holiday = await _fedWebClient.CreateHolidayAsync(new Core.Entities.Holiday(viewModel.Id, viewModel.Name, Date.Parse(viewModel.Date), viewModel.DateAdded));
                if (holiday)
                {
                    ViewData["success"] = "1";
                }
            }
            catch (Exception ex)
            {
                ViewData["Message"] = ex.Message;
                return Redirect("/holidays/getholidays");
            }
            return Redirect("/holidays/getholidays");
        }

        [HttpGet("/holidays/getholidays")]
        public async Task<IActionResult> GetHolidays()
        {
            var startDate = Core.ValueTypes.Date.Today.AddDays(-1);
            var endDate = Core.ValueTypes.Date.Today.AddDays(1825);


            var holidays = await _fedWebClient.GetHolidaysAsync(startDate, endDate);
            if (holidays != null)
            {
                ViewData["success"] = "1";
                return this.PartialView("~/Views/Holidays/Holidays.cshtml", holidays);
            }

            ViewData["success"] = "0";
            return this.PartialView("~/Views/Holidays/Holidays.cshtml", null);
        }

        [HttpPost("/holidays/deleteholiday/{date}")]
        public async Task<IActionResult> DeleteHolidays(string date)
        {

            var deleted = await _fedWebClient.DeleteHolidayAsync(DateTime.Parse(date));
            return Redirect("/holidays/getholidays");
        }
    }
}