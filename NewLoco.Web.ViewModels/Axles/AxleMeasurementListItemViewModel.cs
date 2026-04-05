namespace NewLoco.Web.ViewModels.Axles
{
    public class AxleMeasurementListItemViewModel
    {
        public int Id { get; set; }

        public string DocumentNumber { get; set; } = null!;

        public string LocomotiveNumber { get; set; } = null!;

        public DateTime MeasurementDate { get; set; }

        public int AxleCount { get; set; }

        public bool IsDeleted { get; set; }
    }
}