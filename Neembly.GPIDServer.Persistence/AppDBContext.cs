using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Neembly.GPIDServer.Persistence.Entities;

namespace Neembly.GPIDServer.Persistence
{
    public class AppDBContext : IdentityDbContext<AppUser>
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }
        public DbSet<Player> Players { get; set; }
        public DbSet<OperatorData> OperatorData { get; set; }

    }
}
