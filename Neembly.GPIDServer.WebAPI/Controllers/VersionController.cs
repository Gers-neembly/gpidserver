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
                Version = $"Version {Assembly.GetEntryAssembly().GetName().Version}",
                BuildNo = $"Build {Assembly.GetEntryAssembly().GetName().FullName}",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Cluster = Environment.GetEnvironmentVariable("ASPNETCORE_CLUSTER")
            };
            return new JsonResult(versionTag);
        }
    }
}