using Domain.Entities;
using System.Threading.Tasks;
using Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Infrastructure.Persistance
{
    public class SZRSTContext : IdentityDbContext<ApplicationUser, ApplicationRole, int, ApplicationUserClaim, ApplicationUserRole, ApplicationUserLogin, ApplicationRoleClaim, ApplicationUserToken>, ISZRSTContext
    {
        public SZRSTContext(DbContextOptions options)
         : base(options)
        {
        }

        public Task<int> SaveChangesAsync()
        {
            return base.SaveChangesAsync();
        }

        public override DbSet<TDb> Set<TDb>() where TDb : class
        {
            return base.Set<TDb>();
        }
    }
}
