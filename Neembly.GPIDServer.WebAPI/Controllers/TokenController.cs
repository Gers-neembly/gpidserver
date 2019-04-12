using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Neembly.GPIDServer.Constants;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.SharedServices.Helpers;
using Neembly.GPIDServer.WebAPI.Models.Configs;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        #region Member Variable
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IDataAccess _dataAccess;
        private readonly AuthClientConfiguration _authConfig;
        private readonly TokenProviderService _tokenProviderService;
        #endregion

        #region Constructor
        public TokenController(
            UserManager<AppUser> userManager,
            IConfiguration configuration,
            IDataAccess dataAccess,
            AuthClientConfiguration authConfig,
            TokenProviderService tokenProviderService
            )
        {
            _userManager = userManager;
            _configuration = configuration;
            _dataAccess = dataAccess;
            _authConfig = authConfig;
            _tokenProviderService = tokenProviderService;
        }
        #endregion

        #region Actions
        [Route("GetToken")]
        [HttpGet]
        public async Task<IActionResult> CreateToken(string hostedUrl)
        {
            var success = await _tokenProviderService.CreateToken(GenerateToken(hostedUrl));
            return Ok();
        }
        #endregion


    }
}