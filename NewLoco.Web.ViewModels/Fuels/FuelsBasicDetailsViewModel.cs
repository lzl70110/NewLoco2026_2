using System;

namespace NewLoco.Web.ViewModels.Fuels
{
    public class FuelsBasicDetailsViewModel
    {
        public int Id { get; set; }

        // used for links to locomotive details
        public int LocomotiveId { get; set; }

        public string LocomotiveNumber { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal InitialFuel { get; set; }
        public decimal FinalFuel { get; set; }
        public bool IsDeleted { get; set; }
    }
}