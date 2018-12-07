using Neembly.GPIDServer.SharedClasses;
using System.ComponentModel.DataAnnotations;

namespace Neembly.GPIDServer.Persistence.Entities
{
    public class Player
    {
        [Key]
        public string PlayerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
