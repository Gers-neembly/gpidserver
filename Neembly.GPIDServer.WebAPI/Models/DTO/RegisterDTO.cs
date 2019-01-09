using Neembly.GPIDServer.SharedClasses;
using System.ComponentModel.DataAnnotations;

namespace Neembly.GPIDServer.WebAPI.Model.DTO
{
    public class RegisterDTO
    {
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2}, at max {1} characters long and unique.", MinimumLength = 2)]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string OperatorId { get; set; }

        public string RoleType { get; set; }

        public PlayerInfo playerInfo { get; set; }
    }
}
