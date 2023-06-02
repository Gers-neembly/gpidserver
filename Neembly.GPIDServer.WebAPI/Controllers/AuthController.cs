using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Neembly.GPIDServer.Persistence.Entities;
using Neembly.GPIDServer.WebAPI.Models.DTO.Inputs;
using System.Threading.Tasks;

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
        public IActionResult Login(string returnUrl)
        {


            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            var result = await _signInManager.PasswordSignInAsync(vm.Username, vm.Password, false, false);
            if (result.Succeeded)
                return Redirect(vm.ReturnUrl);
            return View();
        }
    }
}