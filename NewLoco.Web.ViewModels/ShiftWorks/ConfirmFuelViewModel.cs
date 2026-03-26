using NewLoco.GCommon.Enums;

namespace NewLoco.Web.ViewModels.ShiftWorks
{
    public class ConfirmFuelViewModel
    {
        public int LocomotiveId { get; set; }

        public string LocomotiveNumber { get; set; } = string.Empty;

        public LocomotiveType LocomotiveType { get; set; }

        public DateTime Date { get; set; }

        public Shift Shift { get; set; }

        public MeasuringUnits MeasuringUnits { get; set; }
        public decimal InitialValue { get; set; }
        public decimal FinalValue { get; set; }
        public decimal Hours { get; set; }    
        public decimal FuelLiters { get; set; }
        public decimal FullLoadHint { get; set; }
        public decimal PolicyMinLph { get; set; }

        public string? Note { get; set; }
    }
}