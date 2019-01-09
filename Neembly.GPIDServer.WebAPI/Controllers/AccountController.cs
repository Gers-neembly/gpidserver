using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.Persistence.Interfaces;
using Neembly.GPIDServer.SharedClasses;
using Neembly.GPIDServer.WebAPI.Model.DTO;

namespace Neembly.GPIDServer.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IDataAccess _dataAccess;

        public AccountController(
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

        [Route("register")]
        [HttpPost]
        // Description: Registers the new player, will generate a email token based link
        public async Task<object> Register([FromBody] RegisterDTO registerInfo)
        {
            var resultInfo = new ResultsInfo { Success = false, DataInfo = null };

            try
            {
                string clientOperatorId = registerInfo.OperatorId;
                if (string.IsNullOrWhiteSpace(clientOperatorId))
                {
                    resultInfo.ErrorDescription = "operatorid is null";
                    return new JsonResult(resultInfo);
                }

                if (string.IsNullOrWhiteSpace(registerInfo.Email) 
                    || string.IsNullOrWhiteSpace(registerInfo.Password)
                      || string.IsNullOrWhiteSpace(registerInfo.UserName))
                {
                    resultInfo.ErrorDescription = "email or password or username is null";
                    return new JsonResult(resultInfo);
                }

                if (registerInfo.Password != registerInfo.ConfirmPassword)
                {
                    resultInfo.ErrorDescription = "passwords don't match!";
                    return new JsonResult(resultInfo);
                }

                string clientUsername = registerInfo.UserName + '_' + clientOperatorId;

                AppUser player = _dataAccess.GetAppUser(registerInfo.Email, clientUsername, clientOperatorId);
                if (player == null)
                {
                    var user = new AppUser
                    {
                        UserName = clientUsername,
                        OperatorId = clientOperatorId,
                        Email = registerInfo.Email,
                        DisplayUsername = registerInfo.UserName
                    };

                    var result = await _userManager.CreateAsync(user, registerInfo.Password);
                    if (result.Succeeded)
                    {
                        string theRole = string.IsNullOrEmpty(registerInfo.RoleType) ? "player" : registerInfo.RoleType.ToLower();
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("username", user.DisplayUsername));
                        await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("email", user.Email));
                        if (theRole == "player")
                        {
                            user.PlayerId = await _dataAccess.CreatePlayerById(user.Id, user.OperatorId, registerInfo.playerInfo);
                            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("operatorId", user.OperatorId));
                            await _userManager.AddClaimAsync(user, new System.Security.Claims.Claim("playerId", user.PlayerId));
                        }

                        await CreateUserRoles(user, theRole);

                        var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        var callbackUrl = Url.Action(
                            "verifyemail", "account",
                            values: new { userId = user.Id, code = emailConfirmationToken, operatorId = user.OperatorId },
                            protocol: Request.Scheme);
                        resultInfo.DataInfo = callbackUrl;
                        resultInfo.Message = $"Registration completed, please verify your email - {registerInfo.Email}";
                        resultInfo.Success = true;
                    }
                    else
                    {
                        var errorList = result.Errors.ToArray();
                        foreach (var error in errorList)
                        {
                            resultInfo.ErrorDescription = resultInfo.ErrorDescription + $" {error.Description}";
                        }
                    }

                }
                else
                {
                    resultInfo.ErrorDescription = "The email or username you are trying to register already exists.";
                }
            }
            catch (Exception ex)
            {
                resultInfo.ErrorDescription = $"{ex.Message}={ex.InnerException.Message}";
            }
            return new JsonResult(resultInfo);

        }

        [Route("verifyemail")]
        [HttpGet]
        public async Task<IActionResult> VerifyEmail(string userId, string code, string operatorId)
        {

            var resultInfo = new ResultsInfo { Success = false, DataInfo = null };

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    var emailConfirmationResult = await _userManager.ConfirmEmailAsync(user, code);
                    if (!emailConfirmationResult.Succeeded)
                    {
                        var errorList = emailConfirmationResult.Errors.ToArray();
                        foreach (var error in errorList)
                        {
                            resultInfo.ErrorDescription = resultInfo.ErrorDescription + $" {error.Description}";
                        }
                    }
                    else
                    {
                        var callbackUrl = "https://localhost:5001"; 
                        resultInfo.DataInfo = callbackUrl;
                        resultInfo.Message = $"Registration verified, Thank you.";
                        resultInfo.Success = true;
                    }
                }
                else
                {
                    resultInfo.ErrorDescription = "The player information for this verification code does not exists.";
                }

            }
            catch (Exception ex)
            {
                resultInfo.ErrorDescription = $"{ex.Message}={ex.InnerException.Message}";
            }
            return new JsonResult(resultInfo);
        }

        private async Task CreateUserRoles(AppUser user, string roleDesired)
        {
            IdentityResult roleResult;
            var roleCheck = await _roleManager.RoleExistsAsync(roleDesired);
            if (!roleCheck)
            {
                roleResult = await _roleManager.CreateAsync(new IdentityRole(roleDesired));
            }
            await _userManager.AddToRoleAsync(user, roleDesired);
        }

    }
}