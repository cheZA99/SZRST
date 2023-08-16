using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;

namespace Application.Common.Interfaces
{
    public interface ISZRSTContext 
    {
        Task<int> SaveChangesAsync();
        DbSet<TDb> Set<TDb>() where TDb : class;
    }
}
