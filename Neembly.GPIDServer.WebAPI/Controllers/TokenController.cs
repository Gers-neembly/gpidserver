using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedServices.Interfaces;
using Neembly.GPIDServer.WebAPI.Models.Configs;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        #region Member Variable
        private readonly ITokenProviderService _tokenProviderService;
        #endregion

        #region Constructor
        public TokenController(
            ITokenProviderService tokenProviderService
            )
        {
            _tokenProviderService = tokenProviderService;
        }
        #endregion

        #region Actions
         [HttpGet]
        public async Task<IActionResult> Get()
        {
            var success = await _tokenProviderService.CreateToken();
            return Ok();
        }
        #endregion


    }
}