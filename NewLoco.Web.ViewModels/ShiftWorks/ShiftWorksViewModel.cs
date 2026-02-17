using System;
using NewLoco.GCommon.Enums;

namespace NewLoco.Web.ViewModels.ShiftWorks
    {
    // ViewModel for displaying shifts in list or details
    public class ShiftWorksViewModel
        {
        public int Id { get; set; }

        // Locomotive name for display
        public int LocomotiveId { get; set; }
        public string Locomotive { get; set; } = null!;

        // Date of the shift
        public DateTime Date { get; set; }

        // Starting meter value
        public decimal StartValue { get; set; }

        // Ending meter value
        public decimal EndValue { get; set; }

        // Total value calculated from EndValue - StartValue
        public decimal TotalValue => EndValue - StartValue;

        // Shift type (Day/Night)
        public Shift Shift { get; set; }

        // Audit fields
        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        // Optional note
        public string? Note { get; set; }

        // Soft-delete flag
        public bool IsDeleted { get; set; } = false;
         
        }
    }
