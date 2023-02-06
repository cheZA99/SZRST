using Microsoft.EntityFrameworkCore;

namespace SZRST.API.Models
{
    public class SZRSTContext : DbContext
    {
        public SZRSTContext(DbContextOptions<SZRSTContext> options) : base(options)
        {
        }

        public DbSet<Korisnik> Korisnik { get; set; }
        public DbSet<Spol> Spol { get; set; }
        public DbSet<Adresa> Adresa { get; set; }

    }
}
