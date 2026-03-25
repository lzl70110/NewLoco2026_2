// File: NewLoco.Web/ViewModels/Fuels/CreateFuelViewModel.cs
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using static NewLoco.GCommon.EntityValidationConstants.Fuel;

namespace NewLoco.Web.ViewModels.Fuels
{
    public class CreateFuelViewModel : FuelsBasicDetailsViewModel
    {
        [Required]
        public new int LocomotiveId { get; set; }
 
        [ValidateNever]
        public new string? LocomotiveNumber { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public new DateTime Date { get; set; }

     
        [ValidateNever]
        public new decimal InitialFuel { get; set; }
 
        [Required]
        [Range(ValueMin, ValueMax)]
        public new decimal FinalFuel { get; set; }

        
        [Range(ValueMin, ValueMax)]
        public decimal Refueled { get; set; }

     
        [ValidateNever]
        public decimal Consumption { get; set; }

        [StringLength(NoteMaxLength, MinimumLength = 5)]
        public string? Note { get; set; }
    }
}