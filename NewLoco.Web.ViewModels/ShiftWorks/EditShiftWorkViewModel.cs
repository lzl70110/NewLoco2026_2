using System;
using System.ComponentModel.DataAnnotations;
using NewLoco.GCommon.Enums;
using static NewLoco.GCommon.EntityValidationConstants.ShiftWork;
using static NewLoco.GCommon.EntityValidationConstants.BaseEntity;

namespace NewLoco.Web.ViewModels.ShiftWorks
    {
    public class EditShiftWorkViewModel
        {
        [Required]
        public int Id { get; set; }

        [Required]
        public int LocomotiveId { get; set; }

       
        public string LocomotiveNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        public Shift Shift { get; set; }

        [Required]
        [Range(ValueMin, ValueMax)]
        public decimal InitialValue { get; set; }

        [Required]
        [Range(ValueMin, ValueMax)]
        public decimal FinalValue { get; set; }

        [Required]
        [Range(ValueMin, ValueMax)]
        public decimal Amount { get; set; }

        public string? OperatorName { get; set; }

        [StringLength(NoteMaxLength,MinimumLength =NoteMinLength)]
        public string? Note { get; set; }
        }
    }