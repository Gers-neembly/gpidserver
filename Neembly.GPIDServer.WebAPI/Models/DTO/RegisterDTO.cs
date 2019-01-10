using Neembly.GPIDServer.SharedClasses;
using System.ComponentModel.DataAnnotations;

namespace Neembly.GPIDServer.WebAPI.Model.DTO
{
    public class RegisterDTO
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string OperatorId { get; set; }

        public string RoleType { get; set; }

        public PlayerInfo playerInfo { get; set; }
    }
}
