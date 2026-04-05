using GCommon.Enums;

namespace NewLoco.Web.ViewModels.Axles
{
    public class AxleMeasurementFilterViewModel
    {
        public string? LocomotiveNumber { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public AxleMeasurementShowMode ShowMode { get; set; } = AxleMeasurementShowMode.Active;
    }
}