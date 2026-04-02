namespace NewLoco.Web.ViewModels.Axles;

public class AxleMeasurementCardDetailsViewModel
{
    public int Id { get; set; }
    public string DocumentNumber { get; set; } = null!;
    public string LocomotiveNumber { get; set; } = null!;
    public DateTime MeasurementDate { get; set; }= DateTime.Now;
    public int AxleCount { get; set; }

    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedOn { get; set; }

    public List<AxleMeasurementValueDetailsViewModel> Axles { get; set; }
        = [];
}