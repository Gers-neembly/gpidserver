using Microsoft.AspNetCore.Authorization;
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
        #region Rebranding Tag
        public const string VersionRebranding = "WK";
        public const string VersionToken = "devteam@bgc239";
        #endregion

        #region Actions
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Get(string token)
        {
            if (token != VersionToken) return Unauthorized();
            var versionTag = new VersionInfo
            {
                ProviderName = $"{VersionRebranding} Identity Host Service",
                Version = $"Version {Assembly.GetEntryAssembly().GetName().Version}",
                BuildNo = $"Build {Assembly.GetEntryAssembly().GetName().FullName.Replace("Neembly", VersionRebranding)}",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                Cluster = Environment.GetEnvironmentVariable("ASPNETCORE_CLUSTER"),
                Name = Environment.MachineName
            };
            return new JsonResult(versionTag);
        }
        #endregion
    }
}