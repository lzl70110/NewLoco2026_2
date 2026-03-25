using System;
using System.ComponentModel.DataAnnotations;
using NewLoco.GCommon.Enums; // Shift, LocomotiveType  <-- FIXED namespace

namespace NewLoco.Web.ViewModels.ShiftWorks
{
    /// <summary>
    /// Used in the 'confirm fuel' step before persisting the shift.
    /// </summary>
    public class ConfirmFuelViewModel
    {
        // Draft shift (flattened)
        [Required] public int LocomotiveId { get; set; }
        [Required] public DateTime Date { get; set; }
        [Required] public Shift Shift { get; set; }

        [Range(0, 1_000_000)]
        public decimal InitialValue { get; set; }

        [Range(0, 1_000_000)]
        public decimal FinalValue { get; set; }

        [StringLength(2000)]
        public string? Note { get; set; }

        // Derived/runtime
        [Range(0.01, 1_000_000)]
        public decimal Hours { get; set; }

        public LocomotiveType LocomotiveType { get; set; }

        // Fuel confirmation
        [Display(Name = "Fuel (liters)")]
        [Range(0, 1_000_000)]
        public decimal FuelLiters { get; set; }

        // Policy hints
        public decimal PolicyMinLph { get; set; }
        public decimal FullLoadHint { get; set; }
    }
}