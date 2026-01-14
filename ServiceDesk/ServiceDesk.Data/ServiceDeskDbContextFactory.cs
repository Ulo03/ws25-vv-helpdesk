using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceDesk.Data;

public class ServiceDeskDbContextFactory
    : IDesignTimeDbContextFactory<ServiceDeskDbContext>
{
    public ServiceDeskDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ServiceDeskDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=servicedesk;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new ServiceDeskDbContext(options);
    }
}
