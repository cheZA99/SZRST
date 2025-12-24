using Domain.Entities;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using SZRST.Infrastructure.Configurations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Execution;
using SZRST.Domain.Entities;

namespace Infrastructure.Persistance
{
    public partial class SZRSTContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>, ISZRSTContext
    {

        public SZRSTContext()
        {
        }

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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RoleClaimConfiguration).Assembly);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RefreshTokenConfiguration).Assembly);
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
                if (entry.Entity is IBaseEntity<int> entity)
                {
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
        public DbSet<Review> Review { get; set; }
        public DbSet<Worker> Worker { get; set; }
        public DbSet<WorkerType> WorkerType { get; set; }

        public DbSet<AppMember> AppMember { get; set; }
        public DbSet<Photo> Photos { get; set; }


        public DbSet<RefreshToken> RefreshTokens { get; set; }
    }
}
