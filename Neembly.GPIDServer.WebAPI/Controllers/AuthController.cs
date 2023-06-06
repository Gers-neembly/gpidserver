using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.WebAPI.Models.DTO.Inputs;
using System.Threading.Tasks;
using System.Web;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    public class AuthController : Controller
    {
        #region Member Variable
        private readonly SignInManager<AppUser> _signInManager;
        #endregion

        #region Constructor
        public AuthController(
            SignInManager<AppUser> signInManager)
        {
            _signInManager = signInManager;
        }
        #endregion
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var queryString = HttpContext.Request.Query["returnUrl"].ToString();
            var oAuthSignIn = HttpUtility.ParseQueryString(queryString).Get("oAuthSignIn");
            if (!string.IsNullOrEmpty(oAuthSignIn))
            {
                if (await this.ValidateSignInCredentials(oAuthSignIn))
                    return Redirect(returnUrl);
            }
            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            var result = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, false, false);
             if (result.Succeeded)
                return Redirect(vm.ReturnUrl);
            return Unauthorized();
        }

        private async Task<bool> ValidateSignInCredentials(string oAuthSignIn)
        {
            string[] myContextList = oAuthSignIn.Split("$$$");
            LoginViewModel vm = new LoginViewModel { Username = myContextList[0], Password = myContextList[1]};
            if (!string.IsNullOrEmpty(vm.Username) && !string.IsNullOrEmpty(vm.Password))
            {
                var result = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, false, false);
                return result.Succeeded;
            }
            return false;
        }

    }
}