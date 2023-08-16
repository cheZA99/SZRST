using System;
using AutoMapper;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Application.Helpers
{
    public class PagedList<TDb, TModel> : List<TModel>
    {
        protected readonly IMapper _mapper;
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public bool HasPrevious { get { return CurrentPage > 1; } }
        public bool HasNext { get { return CurrentPage > TotalPages; } }

        public PagedList(List<TDb> items, int totalCount, int pageNumber, int pageSize, IMapper mapper)
        {
            _mapper = mapper;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            AddRange(_mapper.Map<IList<TModel>>(items));
        }

        public async static Task<PagedList<TDb, TModel>> CreateAsync(IQueryable<TDb> source, IMapper mapper, int pageNumber = 1, int pageSize = 25)
        {
            var count = source.Count();
            var items = await source.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToListAsync();
            
            return new PagedList<TDb, TModel>(items, count, pageNumber, pageSize, mapper);
        }
    }
}
