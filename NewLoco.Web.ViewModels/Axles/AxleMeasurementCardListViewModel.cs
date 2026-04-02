namespace NewLoco.Web.ViewModels.Axles;

public class AxleMeasurementCardListViewModel
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = null!;
    public string LocomotiveNumber { get; set; } = null!;
    public DateTime MeasurementDate { get; set; }
    public int AxleCount { get; set; }
}