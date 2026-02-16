using System;

namespace NewLoco.Web.ViewModels.Fuels
    {
    public class FuelAllViewModel : FuelsBasicDetailsViewModel
        {
        public int Id { get; set; }
        public string LocomotiveNumber { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal InitialFuel { get; set; }
        public decimal FinalFuel { get; set; }
        public decimal Consumption { get; set; }
        public decimal Refueled { get; set; }
        public string? Note { get; set; }  
        public bool IsDeleted { get; set; }= false;
        }
    }
