using Application.Helpers;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBaseService<T, TDb, TSearch, TInsert, TUpdate> where T : class where TDb : class where TSearch : class where TInsert : class where TUpdate : class
    {
        Task<T> DeleteAsync(int id);
        Task<PagedList<TDb, T>> GetAsync(TSearch search = null);
        Task<T> GetByIdAsync(int id);
        Task<T> InsertAsync(TInsert request);
        Task<T> UpdateAsync(int id, TUpdate request);
    }
}
