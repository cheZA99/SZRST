namespace Application.Requests
{
    public class BaseSearchRequest
    {
        public string TextSearch { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public string SortName { get; set; }
        public string SortDirection { get; set; }
        public string[] IncludeList { get; set; }
    }
}
