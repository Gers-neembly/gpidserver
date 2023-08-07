using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
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
        private readonly IDataAccess _dataAccess;
        #endregion

        #region Constructor
        public AuthController(
            SignInManager<AppUser> signInManager,
            IDataAccess dataAccess)
        {
            _signInManager = signInManager;
            _dataAccess = dataAccess;
        }
        #endregion
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl)
        {
            var queryString = HttpContext.Request.Query["returnUrl"].ToString();
            var username = HttpUtility.ParseQueryString(queryString).Get("username");
            var ssoAuthProvider = HttpUtility.ParseQueryString(queryString).Get("ssoAuthProvider");
            if (!string.IsNullOrEmpty(username))
            {
                if (await this.ValidateSignInCredentials(username, ssoAuthProvider))
                    return Redirect(returnUrl);
                 //   return View();
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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync("idsrv.external");
            await HttpContext.SignOutAsync("idsrv.session"); await HttpContext.SignOutAsync();
            Response.Cookies.Delete("Winka.Identity.Cookie", new CookieOptions()
            {
                Secure = true,
            });
            //Response.Cookies.Delete("idsrv.external", new CookieOptions()
            //{
            //    Secure = true,
            //});
            //Response.Cookies.Delete("idsrv.session", new CookieOptions()
            //{
            //    Secure = true,
            //});
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

        private async Task<bool> ValidateSignInCredentials(string username, string ssoAuthProvider)
        {
            if (!string.IsNullOrEmpty(username))
            {
                if (string.IsNullOrEmpty(ssoAuthProvider))
                {
                    var appUser = await _dataAccess.GetAppUser(username);
                    if (appUser == null) return false;
                    await _signInManager.SignInAsync(appUser, false);
                    return true;
                }
                else return true;
            }
            return false;
        }

    }
}