namespace Application.Responses
{
    public class PagedResponse<T>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public T Items { get; set; }
        public int TotalCount { get; set; }
        public PagedResponse(T items, int currentPage, int pageSize, int totalCount)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            Items = items;
            TotalCount = totalCount;
        }
    }
}