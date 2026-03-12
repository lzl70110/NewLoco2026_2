using System;
using System.ComponentModel.DataAnnotations;
using static NewLoco.GCommon.EntityValidationConstants.Fuel;

namespace NewLoco.Web.ViewModels.Fuels
    {
    public class CreateFuelViewModel : FuelsBasicDetailsViewModel
        {
        [Required]
        public new int LocomotiveId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public new DateTime Date { get; set; }

        public new decimal InitialFuel { get; set; }

        [Required]
        [Range(ValueMin, ValueMax)]
        public new decimal FinalFuel { get; set; }

        [Range(ValueMin, ValueMax)]
        public decimal Refueled { get; set; }

        public decimal Consumption { get; set; }

        [StringLength(NoteMaxLength, MinimumLength = 5)]
        public string? Note { get; set; }
        }
    }