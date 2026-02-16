using System;
using System.ComponentModel.DataAnnotations;
using NewLoco.GCommon.Enums;
using static NewLoco.GCommon.EntityValidationConstants.ShiftWork;
using static NewLoco.GCommon.EntityValidationConstants.BaseEntity;
using NewLoco.Data.Models;
namespace NewLoco.Web.ViewModels.ShiftWorks
    {
    public class CreateShiftWorkViewModel
        {
        

        [Required]
        public int LocomotiveId { get; set; }
        public Locomotive Locomotive { get; set; } = null!;

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

        public DateTime? CreatedOn { get; set; }

        [StringLength(NoteMaxLength, MinimumLength= NoteMinLength)]
        public string? Note { get; set; }

        public string ? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        }
    }