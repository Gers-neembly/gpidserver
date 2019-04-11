using Microsoft.EntityFrameworkCore;
using Neembly.GPIDServer.Persistence.Entities;

namespace Neembly.GPIDServer.Persistence
{
    public class UtilsDBContext : DbContext
    {
        public UtilsDBContext(DbContextOptions<UtilsDBContext> options) : base(options)
        {
        }
        public DbSet<EmailQueue> EmailQueues { get; set; }
    }
}
