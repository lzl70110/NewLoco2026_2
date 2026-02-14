using System;
using System.ComponentModel.DataAnnotations;
using static NewLoco.GCommon.EntityValidationConstants.Fuel;
 

namespace NewLoco.Web.ViewModels.Fuels
    {
    public class CreateFuelViewModel
        {
        [Required]
        public int LocomotiveId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? Date { get; set; }

        [Required]
        [Range(ValueMin, ValueMax)]
        public decimal InitialFuel { get; set; }

        [Required]
        [Range(ValueMin, ValueMax)]
        public decimal FinalFuel { get; set; }

        [Range(ValueMin, ValueMax)]
        public decimal Refueled { get; set; }

        [StringLength(NoteMaxLength,MinimumLength =5)]
        public string? Note { get; set; }
        }
    }
