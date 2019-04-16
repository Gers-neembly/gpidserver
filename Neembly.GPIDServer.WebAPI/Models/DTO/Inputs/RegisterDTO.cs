using Neembly.GPIDServer.SharedClasses;
using System.Collections.Generic;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class RegisterDTO
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public int OperatorId { get; set; }
        public string HostedUrl { get; set; }
        public List<string> Roles { get; set; }
        public PlayerInfo PlayerInfo { get; set; }
        public bool BoUser { get; set; }
        public int PlayerId { get; set; }
    }
}
