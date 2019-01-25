using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Neembly.GPIDServer.Persistence
{
    class UtilsDBContextDesignTimeFactory : IDesignTimeDbContextFactory<UtilsDBContext>
    {
        public UtilsDBContext CreateDbContext(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            IConfigurationRoot configuration = new ConfigurationBuilder()
                                .SetBasePath(Directory.GetCurrentDirectory())
                                .AddJsonFile($"connection.{environmentName}.json")
                                .Build();
            var builder = new DbContextOptionsBuilder<UtilsDBContext>();
            var connectionString = configuration.GetConnectionString("UtilsConnection");
            builder.UseNpgsql(connectionString);
            return new UtilsDBContext(builder.Options);
        }
    }
}
