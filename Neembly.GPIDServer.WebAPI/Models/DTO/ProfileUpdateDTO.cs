using Neembly.GPIDServer.SharedClasses;

namespace Neembly.GPIDServer.WebAPI.Models.DTO
{
    public class ProfileUpdateDTO
    {
        public string PlayerId { get; set; } 
        public PlayerInfo playerInfo { get; set; }
    }
}
