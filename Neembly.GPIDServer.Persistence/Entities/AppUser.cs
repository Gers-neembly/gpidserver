using Microsoft.AspNetCore.Identity;

namespace Neembly.GPIDServer.Persistence.Entities
{
    public class AppUser : IdentityUser
    {
        public string DisplayUsername { get; set; }
        public int OperatorId { get; set; }
        public string PlayerId { get; set; }
        public string RegistrationStatus { get; set; }
    }
}
