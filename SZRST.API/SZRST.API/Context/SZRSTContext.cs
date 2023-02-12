using Microsoft.EntityFrameworkCore;
using SZRST.API.Models;

namespace SZRST.API.Context
{
    public class SZRSTContext : DbContext
    {
        public SZRSTContext(DbContextOptions<SZRSTContext> options) : base(options)
        {
        }

        public DbSet<Korisnik> Korisnik { get; set; }
        public DbSet<Spol> Spol { get; set; }
        public DbSet<Adresa> Adresa { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Korisnik>().ToTable("Korisnik");
            modelBuilder.Entity<Spol>().ToTable("Spol");
            modelBuilder.Entity<Adresa>().ToTable("Adresa");
        }

    }
}
