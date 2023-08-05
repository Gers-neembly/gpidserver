using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.WebAPI.Models.DTO.Inputs;
using System;
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
            var ssoAuthProvider = HttpUtility.ParseQueryString(queryString).Get("ssoAuthProvider");
            if (!string.IsNullOrEmpty(oAuthSignIn))
            {
                if (await this.ValidateSignInCredentials(oAuthSignIn, ssoAuthProvider))
                    return View();
                // this is another way
                //var redirect_uri = HttpUtility.ParseQueryString(queryString).Get("redirect_uri");
                // return Redirect(redirect_uri);
            }
            return Unauthorized();
        }

        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
            await HttpContext.SignOutAsync();
            Response.Cookies.Delete("Winka.Identity.Cookie", new CookieOptions()
            {
                Secure = true,
            });
            return Ok(true);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            var result = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, false, false);
             if (result.Succeeded)
                return Redirect(vm.ReturnUrl);
            return Unauthorized();
        }

        private async Task<bool> ValidateSignInCredentials(string oAuthSignIn, string ssoAuthProvider)
        {
            string[] myContextList = oAuthSignIn.Split("$$$");
            LoginViewModel vm = new LoginViewModel { Username = myContextList[0], Password = myContextList[1]};
            if (!string.IsNullOrEmpty(vm.Username) && !string.IsNullOrEmpty(vm.Password))
            {
                if (string.IsNullOrEmpty(ssoAuthProvider))
                {
                    var result = (await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, false, false)).Succeeded;
                    return result;
                }
                else return true;
            }
            return false;
        }

    }
}