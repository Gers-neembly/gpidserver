using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.WebAPI.Models;
using System;
using System.Reflection;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VersionController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var versionTag = new VersionInfo
            {
                ProviderName = "Neembly Identity Host Service",
                Version = "Version 1.0.0",
                BuildNo = $"Build {Assembly.GetEntryAssembly().GetName().Version}",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            };
            return new JsonResult(versionTag);
        }
    }
}