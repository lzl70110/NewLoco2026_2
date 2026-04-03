namespace NewLoco.Web.ViewModels.ShiftWorks
{
    public class ShiftWorkQuery
    {
        public string? LocomotiveNumber { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public bool IncludeDeleted { get; set; } = false;

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}