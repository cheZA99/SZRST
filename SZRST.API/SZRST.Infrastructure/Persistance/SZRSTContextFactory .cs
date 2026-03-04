using Application.Interfaces;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class SZRSTContextFactory :IDesignTimeDbContextFactory<SZRSTContext>
{
	public SZRSTContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<SZRSTContext>();
        optionsBuilder.UseSqlServer(
    "Server=.;Database=SZRST-app-v2;Trusted_Connection=True;TrustServerCertificate=True;"
);

        var tenantProvider = new DummyTenantProvider();

		return new SZRSTContext(optionsBuilder.Options, tenantProvider);
	}
}

public class DummyTenantProvider :ITenantProvider
{
	public int TenantId => 1;

	public bool IsSuperAdminOrUser => false;
}