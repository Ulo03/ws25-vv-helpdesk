using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceDesk.Data;

public class ServiceDeskDbContextFactory : IDesignTimeDbContextFactory<ServiceDeskDbContext>
{
    public ServiceDeskDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ServiceDeskDbContext>();

        optionsBuilder.UseNpgsql("Host=localhost;Port=52748;Database=servicedesk;Username=postgres;Password=postgres");

        return new ServiceDeskDbContext(optionsBuilder.Options);
    }
}
