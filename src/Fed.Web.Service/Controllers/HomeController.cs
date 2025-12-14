using Fed.Core.Entities;
using Fed.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Fed.Web.Service.Controllers
{
    [ApiController]
    public class HomeController : ControllerBase
    {
        /// <summary>
        /// Health check to test if the web service is available.
        /// </summary>
        [HttpGet("/ping")]
        [HttpPost("/ping")]
        public ActionResult<string> Ping()
        {
            return Ok("pong");
        }

        /// <summary>
        /// Returns version and build number of the API.
        /// </summary>
        [HttpGet("/info")]
        public ActionResult<AppInfo> Info()
        {
            var assembly = typeof(Startup).Assembly;

            var creationDate = System.IO.File.GetCreationTime(assembly.Location);
            var version = FileVersionInfo.GetVersionInfo(assembly.Location).ProductVersion;

            return Ok(new AppInfo { Version = version, LastPublished = creationDate });
        }

        /// <summary>
        /// Returns a list of possible error codes returned by the API.
        /// </summary>
        [HttpGet("/error-codes")]
        public ActionResult<IDictionary<int, string>> ErrorCodes()
        {
            var values = Enum
                .GetValues(typeof(ErrorCode))
                .Cast<ErrorCode>();

            var result = new Dictionary<int, string>();

            foreach (var v in values)
                result.Add((int)v, v.ToString());

            return result;
        }
    }
}