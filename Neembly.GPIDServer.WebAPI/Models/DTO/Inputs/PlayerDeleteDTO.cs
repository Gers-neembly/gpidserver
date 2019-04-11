using Neembly.GPIDServer.SharedClasses;
using System.Collections.Generic;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class PlayerDeleteDTO
    {
        public int OperatorId { get; set; }
        public int PlayerId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
    }
}
