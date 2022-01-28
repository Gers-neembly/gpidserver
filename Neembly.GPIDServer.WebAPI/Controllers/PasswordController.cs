using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Neembly.GPIDServer.Constants;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.WebAPI.Filters;
using Neembly.GPIDServer.WebAPI.Models.DTO.Inputs;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        #region Member Variable
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IDataAccess _dataAccess;
        #endregion

        #region Constructor
        public PasswordController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            IDataAccess dataAccess
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _dataAccess = dataAccess;
        }
        #endregion

        #region Actions

        #region Change
        [NeemblyAuthorize]
        [Route("change")]
        [HttpPost]
        public async Task<IActionResult> Change([FromBody] ChangePasswordDTO changePassWordInfo)
        {
            string userName = $"{changePassWordInfo.UserName}_{changePassWordInfo.OperatorId}";
            AppUser ppUser;

            if (!string.IsNullOrEmpty(changePassWordInfo.Email))
                ppUser = _dataAccess.GetAppUser(changePassWordInfo.Email, userName);
            else
                ppUser = await _dataAccess.GetAppUser(userName);

            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            var result = await _userManager.ChangePasswordAsync(ppUser, changePassWordInfo.CurrentPassword, changePassWordInfo.NewPassword);
            return Ok(result);
        }
        #endregion

        #region Reset
        [Route("reset")]
        [HttpGet]
        public async Task<IActionResult> Reset(string userName, string email, string token, string newPassword, string operatorId, string homepage)
        {
            string username = $"{userName}_{operatorId}";
            AppUser ppUser;

            if (!string.IsNullOrEmpty(email))
                ppUser = _dataAccess.GetAppUser(email, username);
            else
                ppUser = await _dataAccess.GetAppUser(username);
            
            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            token = token.Replace(" ", "+");
            var result = await _userManager.ResetPasswordAsync(ppUser, token, newPassword);
            if (!result.Succeeded)
                return BadRequest(result);
            if (!string.IsNullOrEmpty(homepage))
                return Redirect(homepage);
            else return Ok(result);
        }
        #endregion

        #region Reset
        [NeemblyAuthorize]
        [Route("reset/token")]
        [HttpPost]
        public async Task<IActionResult> ResetToken([FromBody] ResetPasswordTokenDTO resetPasswordToken)
        {
            string userName = $"{resetPasswordToken.UserName}_{resetPasswordToken.OperatorId}";
            AppUser ppUser = _dataAccess.GetAppUser(resetPasswordToken.Email, userName);
                        
            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            var result = await _userManager.GeneratePasswordResetTokenAsync(ppUser);
            return Ok(result);
        }
        #endregion

        #region Reset
        [NeemblyAuthorize]
        [Route("reset/link")]
        [HttpPost]
        public async Task<IActionResult> ResetTokenLink([FromBody] ResetPasswordAutoTokenDTO resetPasswordToken)
        {
            string userName = $"{resetPasswordToken.UserName}_{resetPasswordToken.OperatorId}";
            AppUser ppUser = await _dataAccess.GetAppUser(userName);
            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            var token = await _userManager.GeneratePasswordResetTokenAsync(ppUser);
            ResetPasswordDTO resetPasswordEntity = new ResetPasswordDTO
            {
                UserName = resetPasswordToken.UserName,
                Email = resetPasswordToken.Email,
                OperatorId = resetPasswordToken.OperatorId,
                Token = token,
                NewPassword = resetPasswordToken.NewPassword,
                HomePage = resetPasswordToken.HomePage
            };
            var link = $"{HttpContext.Request.Scheme}://{resetPasswordToken.HomePage}/reset-password-bo/{Uri.EscapeDataString(token)}?username={resetPasswordToken.UserName}&newPassword={resetPasswordToken.NewPassword}";
            return Ok(link);
        }
        #endregion

        #region Reset
        [Route("reset/link")]
        [HttpGet]
        public async Task<IActionResult> ResetPasswordLink(int operatorId, string operatorDomain, string username)
        {
            string userName = $"{username}_{operatorId}";
            AppUser ppUser = await _dataAccess.GetAppUser(userName);

            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);

            var token = await _userManager.GeneratePasswordResetTokenAsync(ppUser);
            var link = $"{HttpContext.Request.Scheme}://{operatorDomain}/reset-password/{Uri.EscapeDataString(token)}?username={username}";
            return Ok(link);
        }
        #endregion

        #region Reset
        [NeemblyAuthorize]
        [Route("reset/autoToken")]
        [HttpGet]
        public async Task<IActionResult> ResetAutoToken([FromBody] ResetPasswordAutoTokenDTO resetPassword)
        {
            string userName = $"{resetPassword.UserName}_{resetPassword.OperatorId}";
            AppUser ppUser = _dataAccess.GetAppUser(resetPassword.Email, userName);
            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            var token = await _userManager.GeneratePasswordResetTokenAsync(ppUser);
            var result = await _userManager.ResetPasswordAsync(ppUser, token, resetPassword.NewPassword);
            return Ok(result);
        }

        [NeemblyAuthorize]
        [Route("reset/forced")]
        [HttpPost]
        public async Task<IActionResult> ResetPasswordForced([FromBody] ResetPasswordAutoTokenDTO resetPassword)
        {
            string userName = $"{resetPassword.UserName}_{resetPassword.OperatorId}";
            AppUser ppUser = await _dataAccess.GetAppUser(userName);
            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            var token = await _userManager.GeneratePasswordResetTokenAsync(ppUser);
            var result = await _userManager.ResetPasswordAsync(ppUser, token, resetPassword.NewPassword);
            return Ok(result);
        }
        #endregion

        #region Verify Token Reset Password
        [Route("verify-token/reset-password")]
        [HttpGet]
        public async Task<IActionResult> VerifyTokenResetPassword(int operatorId, string username, string token)
        {
            string userName = $"{username}_{operatorId}";
            AppUser ppUser = await _dataAccess.GetAppUser(userName);
            token = token.Replace(" ", "+");
            var result = await _userManager.VerifyUserTokenAsync(ppUser, _userManager.Options.Tokens.PasswordResetTokenProvider, "ResetPassword", token);
            return Ok(result);
        }
        #endregion

        #endregion
    }
}