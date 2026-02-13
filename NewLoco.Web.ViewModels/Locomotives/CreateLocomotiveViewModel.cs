using System.ComponentModel.DataAnnotations;
using NewLoco.GCommon.Enums;
using static NewLoco.GCommon.EntityValidationConstants.Locomotive;
using static NewLoco.GCommon.EntityValidationConstants.BaseEntity;

namespace NewLoco.Web.ViewModels.Locomotives
    {
    public class CreateLocomotiveViewModel
        {
        [Required]
        [StringLength(LocomotiveNumberLength)]
        public string Number { get; set; } = null!;

        [Required]
        public LocomotiveType LocomotiveType { get; set; } = LocomotiveType.Shunter;

        [Required]
        public MeasuringUnits MeasuringUnit { get; set; } = MeasuringUnits.Mh;

        [StringLength(NoteMaxLength, MinimumLength = NoteMinLength)]
        public string? Note { get; set; }
        }
    }