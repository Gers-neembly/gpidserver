using Neembly.GPIDServer.SharedClasses;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ProfileUpdateDTO
    {
        public string PlayerId { get; set; } 
        public PlayerInfo PlayerInfo { get; set; }
    }
}
