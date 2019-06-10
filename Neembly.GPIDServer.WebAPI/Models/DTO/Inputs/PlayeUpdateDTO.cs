using Neembly.GPIDServer.SharedClasses;
using System.Collections.Generic;

namespace Neembly.GPIDServer.WebAPI.Models.DTO.Inputs
{
    public class PlayerUpdateDTO
    {
        public int PlayerId { get; set; }
        public int OperatorId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ModifiedBy { get; set; }
    }
}