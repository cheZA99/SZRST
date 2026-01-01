using Application.Interfaces;
using Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class SZRSTContextFactory :IDesignTimeDbContextFactory<SZRSTContext>
{
	public SZRSTContext CreateDbContext(string[] args)
	{
		var optionsBuilder = new DbContextOptionsBuilder<SZRSTContext>();
		optionsBuilder.UseSqlServer("Server=Amer-PC\\MSSQLSERVER01;Database=SZRST;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;");

		var tenantProvider = new DummyTenantProvider();

		return new SZRSTContext(optionsBuilder.Options, tenantProvider);
	}
}

public class DummyTenantProvider :ITenantProvider
{
	public int TenantId => 1;

	public bool IsSuperAdminOrUser => false;
}