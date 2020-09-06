using System.Threading.Tasks;
using System.Web;
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

            var result = await _userManager.ResetPasswordAsync(ppUser, token, newPassword);
            if (!result.Succeeded)
                return BadRequest(result);
            if (!string.IsNullOrEmpty(homepage))
                return Redirect(homepage);
            else return Ok(result);
        }
        #endregion

        #region Reset
        [Route("reset/token")]
        [HttpPost]
        public async Task<IActionResult> ResetToken([FromBody] ResetPasswordTokenDTO resetPasswordToken)
        {
            string userName = $"{resetPasswordToken.UserName}_{resetPasswordToken.OperatorId}";
            AppUser ppUser;

            if (!string.IsNullOrEmpty(resetPasswordToken.Email))
                ppUser = _dataAccess.GetAppUser(resetPasswordToken.Email, userName);
            else
                ppUser = await _dataAccess.GetAppUser(userName);

            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            var result = await _userManager.GeneratePasswordResetTokenAsync(ppUser);
            return Ok(HttpUtility.UrlEncode(result));
        }
        #endregion

        #region Reset
        [NeemblyAuthorize]
        [Route("reset/link")]
        [HttpPost]
        public async Task<IActionResult> ResetTokenLink([FromBody] ResetPasswordAutoTokenDTO resetPasswordToken)
        {
            string userName = $"{resetPasswordToken.UserName}_{resetPasswordToken.OperatorId}";
            AppUser ppUser = _dataAccess.GetAppUser(resetPasswordToken.Email, userName);
            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            var tokenLink = await _userManager.GeneratePasswordResetTokenAsync(ppUser);
            ResetPasswordDTO resetPasswordEntity = new ResetPasswordDTO
            {
                UserName = resetPasswordToken.UserName,
                Email = resetPasswordToken.Email,
                OperatorId = resetPasswordToken.OperatorId,
                Token = tokenLink,
                NewPassword = resetPasswordToken.NewPassword,
                HomePage = resetPasswordToken.HomePage
            };
            var result = Url.Action("Reset",
                                "Password", resetPasswordEntity,
                                 protocol: HttpContext.Request.Scheme);
            return Ok(result);
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
        #endregion

        #endregion
    }
}