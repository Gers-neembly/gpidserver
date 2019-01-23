using Neembly.GPIDServer.SharedClasses;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Neembly.GPIDServer.WebAPI.Models.DTO
{
    public class RegisterDTO
    {
        [JsonRequired]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [JsonRequired]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [JsonRequired]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [JsonRequired]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        [JsonRequired]
        public string OperatorId { get; set; }

        [JsonRequired]
        public string HostedUrl { get; set; }

        public string RoleType { get; set; }

        public PlayerInfo playerInfo { get; set; }
    }
}
