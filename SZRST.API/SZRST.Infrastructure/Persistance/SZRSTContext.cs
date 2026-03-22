using Application.Common.Interfaces;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SZRST.Domain.Entities;
using SZRST.Infrastructure.Configurations;

namespace Infrastructure.Persistance
{
	public partial class SZRSTContext :IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>, ISZRSTContext
	{
		private readonly ITenantProvider _tenantProvider;

		public SZRSTContext(DbContextOptions<SZRSTContext> options, ITenantProvider tenantProvider)
    : base(options)
		{
			_tenantProvider = tenantProvider ?? new DummyTenantProvider(); // fallback
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseEntityTypeConfiguration<>).Assembly);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserRoleConfiguration).Assembly);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserTokenConfiguration).Assembly);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserClaimConfiguration).Assembly);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserLoginConfiguration).Assembly);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(RoleClaimConfiguration).Assembly);
			modelBuilder.ApplyConfigurationsFromAssembly(typeof(RefreshTokenConfiguration).Assembly);
			modelBuilder.Entity<AppointmentType>()
				.Property(x => x.Price)
				.HasColumnType("decimal(18,2)");

			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
				{
					if (entityType.ClrType == typeof(User) ||
					    entityType.ClrType == typeof(Role) ||
					    entityType.ClrType == typeof(UserRole) ||
					    entityType.ClrType == typeof(UserClaim) ||
					    entityType.ClrType == typeof(UserLogin) ||
					    entityType.ClrType == typeof(UserToken) ||
					    entityType.ClrType == typeof(City) ||
					    entityType.ClrType == typeof(FacilityType) ||
					    entityType.ClrType == typeof(Country) ||
					    entityType.ClrType == typeof(Currency) ||
					    entityType.ClrType == typeof(RefreshToken) ||
					    entityType.ClrType == typeof(RoleClaim))
					{
						continue;
					}

					if (entityType.ClrType == typeof(Reservation) ||
					    entityType.ClrType == typeof(Facility) ||
					    entityType.ClrType == typeof(Location) ||
                        entityType.ClrType == typeof(ReservationReport))
					{
						var method = typeof(SZRSTContext)
						    .GetMethod(nameof(SetConditionalTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)
						    .MakeGenericMethod(entityType.ClrType);
						method.Invoke(this, new object[] { modelBuilder });
					}
					else
					{
						var method = typeof(SZRSTContext)
						    .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Instance)
						    .MakeGenericMethod(entityType.ClrType);
						method.Invoke(this, new object[] { modelBuilder });
					}
				}
			}
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
		{
			ModifyTimestamps();

			foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
			{
				//if (entry.State == EntityState.Added)
				//{
				//	// Ako TenantProvider nema TenantId (npr. SuperAdmin), postavi na 1 (core tenant)
				//	entry.Entity.TenantId = _tenantProvider.TenantId == 0 || _tenantProvider.TenantId == null
				//	    ? 1
				//	    : _tenantProvider.TenantId;
				//}
			}
			return base.SaveChangesAsync(cancellationToken);
		}

		private void ModifyTimestamps()
		{
			var entries = ChangeTracker.Entries();
			foreach (var entry in entries)
			{
				if (entry.Entity is IBaseEntity<int> entity)
				{
					if (entry.State == EntityState.Added)
					{
						entity.DateCreated = DateTime.UtcNow;
					}
					else if (entry.State == EntityState.Modified)
					{
						entity.DateModified = DateTime.UtcNow;
					}
				}
			}
		}

		private void SetTenantFilter<TEntity>(ModelBuilder builder)
    where TEntity : class, ITenantEntity
		{
			builder.Entity<TEntity>()
			    .HasQueryFilter(e =>
					_tenantProvider.IsSuperAdminOrUser ||
					(_tenantProvider.TenantId > 0 && e.TenantId == _tenantProvider.TenantId));
		}

		private void SetConditionalTenantFilter<TEntity>(ModelBuilder builder)
		    where TEntity : class, ITenantEntity
		{
			builder.Entity<TEntity>()
			    .HasQueryFilter(e =>
				   _tenantProvider.IsSuperAdminOrUser ||
				   (_tenantProvider.TenantId > 0 && e.TenantId == _tenantProvider.TenantId));
		}

		public override DbSet<TDb> Set<TDb>() where TDb : class
		{
			return base.Set<TDb>();
		}

		public DbSet<Currency> Currency { get; set; }
		public DbSet<Country> Country { get; set; }
		public DbSet<City> City { get; set; }
		public DbSet<Location> Location { get; set; }
		public DbSet<Facility> Facility { get; set; }
		public DbSet<FacilityType> FacilityType { get; set; }
		public DbSet<Appointment> Appointment { get; set; }
		public DbSet<AppointmentType> AppointmentType { get; set; }
		public DbSet<Reservation> Reservation { get; set; }
        public DbSet<ReservationReport> ReservationReport { get; set; }
        public DbSet<Review> Review { get; set; }
		public DbSet<Worker> Worker { get; set; }
		public DbSet<WorkerType> WorkerType { get; set; }

		public DbSet<AppMember> AppMembers { get; set; }
		public DbSet<Photo> Photos { get; set; }

		public DbSet<RefreshToken> RefreshTokens { get; set; }
	}
}
