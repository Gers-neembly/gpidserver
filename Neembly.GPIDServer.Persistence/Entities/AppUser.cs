using Microsoft.AspNetCore.Identity;

namespace Neembly.GPIDServer.Persistence.Entities
{
    public class AppUser : IdentityUser
    {
        //Extended proprties 
        public string OperatorId { get; set; }
        public string PlayerId { get; set; }
    }
}
