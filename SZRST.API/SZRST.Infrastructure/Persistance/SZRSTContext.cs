using Domain.Entities;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using SZRST.Infrastructure.Configurations;

namespace Infrastructure.Persistance
{
    public partial class SZRSTContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>, ISZRSTContext
    { 

        public SZRSTContext(DbContextOptions options)
         : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BaseEntityTypeConfiguration<>).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserRoleConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserTokenConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserClaimConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserLoginConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RoleClaimConfiguration).Assembly);
        }

        public Task<int> SaveChangesAsync()
        {
            ModifyTimestamps();
            return base.SaveChangesAsync();
        }

        private void ModifyTimestamps()
        {
            var entries = ChangeTracker.Entries();

            foreach (var entry in entries)
            {
                var entity = ((IBaseEntity<int>)entry.Entity);

                if (entry.State == EntityState.Added)
                {
                    entity.DateCreated = DateTime.Now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.DateModified = DateTime.Now;
                }
            }
        }
        public override int SaveChanges()
        {
            ModifyTimestamps();
            return base.SaveChanges();
        }

        public override DbSet<TDb> Set<TDb>() where TDb : class
        {
            return base.Set<TDb>();
        }
    }
}
