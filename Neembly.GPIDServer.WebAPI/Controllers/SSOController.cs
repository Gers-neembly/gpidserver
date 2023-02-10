using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.SharedServices.Interfaces;
using Neembly.GPIDServer.SharedServices.SSO;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSOController : ControllerBase
    {
        #region Member Variable
        private readonly ISSOService _ssoService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        #endregion

        #region Constructor
        public SSOController(
            ISSOService ssoService,
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager            )
        {
            _ssoService = ssoService;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        #endregion

        #region Actions
        [DisableCors]
        [Authorize(AuthenticationSchemes = GoogleDefaults.AuthenticationScheme)]
        [Route("login-google")]
        [HttpGet]
        public IActionResult Get(int id)
        {

            var userClaims = _ssoService.GetSSOUserInfo(this.User);
            return Ok(userClaims ?? null); ;
        }
        #endregion

    }

}