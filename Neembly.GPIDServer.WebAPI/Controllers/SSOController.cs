using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.SharedServices.SSO;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SSOController : ControllerBase
    {
        #region Member Variable
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        #endregion

        #region Constructor
        public SSOController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        #endregion

        #region Actions
        [Authorize(AuthenticationSchemes = GoogleDefaults.AuthenticationScheme)]
        [Route("login-google")]
        [HttpGet]
        public IActionResult Get(int id)
        {

            var userClaims = UserClaimsExtend.GetSSOUserInfo(this.User);
            return Ok(userClaims ?? null); ;
        }
        #endregion

    }

}