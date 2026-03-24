using Application.Interfaces;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

public class SZRSTContextFactory : IDesignTimeDbContextFactory<SZRSTContext>
{
    public SZRSTContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("SZRST");

        var optionsBuilder = new DbContextOptionsBuilder<SZRSTContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var tenantProvider = new DummyTenantProvider();

        return new SZRSTContext(optionsBuilder.Options, tenantProvider);
    }
}

public class DummyTenantProvider : ITenantProvider
{
    public int TenantId => 1;

    public bool IsSuperAdminOrUser => false;
}