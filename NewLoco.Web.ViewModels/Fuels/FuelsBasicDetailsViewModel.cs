using System;

namespace NewLoco.Web.ViewModels.Fuels
    {
    public class FuelsBasicDetailsViewModel 
        {
        public DateTime CreatedOn { get; set; }
        public string CreatedByUserName { get; set; } = null!;
        public string? EditedBy { get; set; }
        public DateTime? EditedOn { get; set; }
        }
    }
