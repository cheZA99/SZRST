using System;
using AutoMapper;
using Domain.Entities;
using Application.Helpers;
using Application.Requests;
using Application.Interfaces;
using System.Threading.Tasks;
using Application.Common.Interfaces;

namespace Application.Services
{
    public class BaseService<T, TSearch, TDb, TInsert, TUpdate> : IBaseService<T, TDb, TSearch, TInsert, TUpdate> where T : class where TSearch : class where TInsert : class where TUpdate : class where TDb : BaseEntity<int> 
    {
        protected readonly ISZRSTContext _context;
        protected readonly IMapper _mapper;
        public BaseService(ISZRSTContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public virtual async Task<PagedList<TDb, T>> GetAsync(TSearch search = null)
        {
            var entity = _context.Set<TDb>();
            if(search is BaseSearchRequest baseSearchRequest)
            {
                return await PagedList<TDb, T>.CreateAsync(entity.AsQueryable(), _mapper, baseSearchRequest.CurrentPage, baseSearchRequest.PageSize);
            }
            return await PagedList<TDb, T>.CreateAsync(entity.AsQueryable(), _mapper);
        }

        public virtual async Task<T> DeleteAsync(int id)
        {
            var set = _context.Set<TDb>();
            var entity = await set.FindAsync(id);
            set.Remove(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<T>(entity);
        }
        public virtual async Task<T> GetByIdAsync(int id)
        {
            var set = _context.Set<TDb>();
            var entity = await set.FindAsync(id);
            return _mapper.Map<T>(entity);
        }
        public virtual async Task<T> InsertAsync(TInsert request)
        {
            var set = _context.Set<TDb>();
            TDb entity = _mapper.Map<TDb>(request);
            entity.DateCreated = DateTime.Now;
            set.Add(entity);
            await _context.SaveChangesAsync();
            return _mapper.Map<T>(entity);
        }
        public virtual async Task<T> UpdateAsync(int id, TUpdate request)
        {
            var set = _context.Set<TDb>();
            var entity = await set.FindAsync(id);
            _mapper.Map(request, entity);
            entity.DateModified = DateTime.Now;
            await _context.SaveChangesAsync();
            return _mapper.Map<T>(entity);
        }

    }
}
