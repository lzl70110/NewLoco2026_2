namespace NewLoco.Web.ViewModels.Paging
{
    public sealed class PagingInfo
    {
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public int TotalItems { get; init; }

        public int TotalPages => (int)System.Math.Ceiling((double)TotalItems / System.Math.Max(1, PageSize));
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}