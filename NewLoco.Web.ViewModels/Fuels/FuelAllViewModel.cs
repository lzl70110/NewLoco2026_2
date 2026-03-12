using System;

namespace NewLoco.Web.ViewModels.Fuels
{
    // inherits: Id, LocomotiveId, LocomotiveNumber, Date, InitialFuel, FinalFuel, IsDeleted
    public class FuelAllViewModel : FuelsBasicDetailsViewModel
    {
        public decimal Consumption { get; set; }
        public decimal Refueled { get; set; }
        public string? Note { get; set; }

        // audit (details view)
        public DateTime CreatedOn { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime? EditedOn { get; set; }
        public string? EditedBy { get; set; }
    }
}