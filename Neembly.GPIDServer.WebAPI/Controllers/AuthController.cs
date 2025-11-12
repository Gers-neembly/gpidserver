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
using System.Net.Http;
using System.Net.Http.Headers;
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
            if (await this.ValidateSignInCredentials(username))
                return Redirect(returnUrl);
            else
                return Unauthorized();
            // this is another way
            //var redirect_uri = HttpUtility.ParseQueryString(queryString).Get("redirect_uri");
            // return Redirect(redirect_uri);
        }

        [HttpGet]
        public IActionResult AccessDenied(string returnUrl)
        {
            var authProvider = HttpContext.Request.Query["oAuth"].ToString();
            string urlReferer = Request.Headers["Referer"].ToString();
            var responseContent = $"<html><center><h2>{urlReferer}<br><br>{authProvider} Login Cancelled <br><br> Close window to go back</h2></center></html>";
            return Content(responseContent, "text/html");
        }

        public void RemoveCookie(string key)
        {
            //Erase the data in the cookie
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddDays(-1);
            option.Secure = true;
            option.IsEssential = true;
            Response.Cookies.Append(key, string.Empty, option);
            //Then delete the cookie
            Response.Cookies.Delete(key);
        }

        [HttpGet]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
            await HttpContext.SignOutAsync(IdentityServerConstants.DefaultCookieAuthenticationScheme);
            await HttpContext.SignOutAsync();
            RemoveCookie("IdServer.GP.Identity.Cookie");
            RemoveCookie("idsrv.external");
            RemoveCookie("idsrv.session");
            return Ok(true);
        }

        [HttpGet]
        [Route("cookie-signout")]
        public IActionResult UserLogout(string returnUrl, string token)
        {
            if (token == "winkaprop239")
            {
                RemoveCookie("IdServer.GP.Identity.Cookie");
                RemoveCookie("idsrv.external");
                RemoveCookie("idsrv.session");
            }
            return Redirect(returnUrl);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            var result = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, false, false);
             if (result.Succeeded)
                return Redirect(vm.ReturnUrl);
            return Unauthorized();
        }

        /// <summary>
        /// Flexible login endpoint that supports login with either email or phone number.
        /// This endpoint was copied from the original Login method and modified to support
        /// consolidated phone/email login where only one contact method is required.
        ///
        /// Flow:
        /// - Email login: Finds user by email and operator, then authenticates
        /// - Phone login: Finds user by phone number and operator, then authenticates
        /// </summary>
        [HttpPost]
        [Route("login-flexible")]
        public async Task<IActionResult> FlexibleLogin(FlexibleLoginDTO loginInfo)
        {
            AppUser user = null;

            // Find user by email or phone number for the specific operator
            if (!string.IsNullOrEmpty(loginInfo.Email))
            {
                // Login with email
                user = await _dataAccess.GetAppUserOnOperator(loginInfo.Email, loginInfo.OperatorId);
            }
            else if (!string.IsNullOrEmpty(loginInfo.PhoneNumber))
            {
                // Login with phone number - use proper phone lookup
                user = await _dataAccess.GetAppUserByPhoneOnOperator(loginInfo.PhoneNumber, loginInfo.OperatorId);
            }

            if (user == null)
                return Unauthorized();

            // Verify password
            var result = await _signInManager.PasswordSignInAsync(user.UserName, loginInfo.Password, false, false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(loginInfo.ReturnUrl))
                    return Redirect(loginInfo.ReturnUrl);
                else
                    return Ok(new { Message = "Login successful", PlayerId = user.PlayerId });
            }

            return Unauthorized();
        }

        private async Task<bool> ValidateSignInCredentials(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                var appUser = await _dataAccess.GetAppUser(username);
                if (appUser != null)
                {
                    await _signInManager.SignInAsync(appUser, false);
                    return true;
                }
            }
            return false;
        }

    }
}