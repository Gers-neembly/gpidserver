using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Neembly.GPIDServer.Constants;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
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
        [Route("change")]
        [HttpPost]
        public async Task<IActionResult> Change([FromBody] ChangePasswordDTO changePassWordInfo)
        {
            string userName = $"{changePassWordInfo.UserName}_{changePassWordInfo.OperatorId}";
            AppUser ppUser = _dataAccess.GetAppUser(changePassWordInfo.Email, userName);
            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            var result = await _userManager.ChangePasswordAsync(ppUser, changePassWordInfo.CurrentPassword, changePassWordInfo.NewPassword);
            return Ok(result);
        }
        #endregion

        #region Change
        [Route("reset")]
        [HttpPost]
        public async Task<IActionResult> Reset([FromBody] ResetPasswordDTO changePassWordInfo)
        {
            string userName = $"{changePassWordInfo.UserName}_{changePassWordInfo.OperatorId}";
            AppUser ppUser = _dataAccess.GetAppUser(changePassWordInfo.Email, userName);
            if (ppUser == null)
                return NotFound(GlobalConstants.ErrUserAccountNotExisting);
            var result = await _userManager.ResetPasswordAsync(ppUser, changePassWordInfo.Token, changePassWordInfo.NewPassword);
            return Ok(result);
        }
        #endregion

        #endregion
    }
}