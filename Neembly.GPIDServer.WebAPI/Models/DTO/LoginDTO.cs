using System.ComponentModel.DataAnnotations;

namespace Neembly.GPIDServer.WebAPI.Models.DTO
{
    public class LoginDTO
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string OperatorId { get; set; }

    }
}
