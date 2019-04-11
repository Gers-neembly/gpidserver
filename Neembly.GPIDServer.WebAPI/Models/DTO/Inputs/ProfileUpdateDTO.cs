using Neembly.GPIDServer.SharedClasses;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class ProfileUpdateDTO
    {
        public int PlayerId { get; set; }
        public int OperatorId { get; set; }
        public PlayerInfo PlayerInfo { get; set; }
    }
}
