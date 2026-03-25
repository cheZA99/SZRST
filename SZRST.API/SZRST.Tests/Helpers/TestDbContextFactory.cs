using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistance;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SZRST.Tests.Helpers
{
	public class DummyTenantProvider :ITenantProvider
	{
		public DummyTenantProvider(int tenantId = 0, bool isSuperAdminOrUser = true)
		{
			TenantId = tenantId;
			IsSuperAdminOrUser = isSuperAdminOrUser;
		}

		public int TenantId { get; }
		public bool IsSuperAdminOrUser { get; set; }
	}

	public static class TestDbContextFactory
	{
		public static SZRSTContext Create(string dbName)
		{
			return Create(dbName, new DummyTenantProvider(tenantId: 1, isSuperAdminOrUser: false));
		}

		public static SZRSTContext CreateSuperAdmin(string dbName)
		{
			return Create(dbName, new DummyTenantProvider(tenantId: 0, isSuperAdminOrUser: true));
		}

		public static SZRSTContext Create(string dbName, int tenantId, bool isSuperAdminOrUser = false)
		{
			return Create(dbName, new DummyTenantProvider(tenantId, isSuperAdminOrUser));
		}

		public static SZRSTContext Create(string dbName, ITenantProvider tenantProvider)
		{
			var options = new DbContextOptionsBuilder<SZRSTContext>()
			    .UseInMemoryDatabase(databaseName: dbName)
			    .Options;

			var context = new SZRSTContext(options, tenantProvider);
			context.Database.EnsureCreated();
			return context;
		}
	}

	public static class TestUserManagerFactory
	{
		public static UserManager<User> Create(SZRSTContext context)
		{
			var store = new UserStore<User, Role, SZRSTContext, int,
			    UserClaim, UserRole, UserLogin, UserToken, RoleClaim>(context);

			var options = Microsoft.Extensions.Options.Options.Create(new IdentityOptions
			{
				Password = new PasswordOptions
				{
					RequireDigit = false,
					RequiredLength = 6,
					RequireLowercase = false,
					RequireUppercase = false,
					RequireNonAlphanumeric = false
				}
			});

			var userManager = new UserManager<User>(
			    store,
			    options,
			    new PasswordHasher<User>(),
			    new List<IUserValidator<User>> { new UserValidator<User>() },
			    new List<IPasswordValidator<User>> { new PasswordValidator<User>() },
			    new UpperInvariantLookupNormalizer(),
			    new IdentityErrorDescriber(),
			    null!,
			    new Microsoft.Extensions.Logging.Abstractions.NullLogger<UserManager<User>>()
			);

			return userManager;
		}
	}
}
