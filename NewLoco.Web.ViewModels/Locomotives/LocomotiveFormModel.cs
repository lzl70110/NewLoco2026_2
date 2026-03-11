using System.ComponentModel.DataAnnotations;
using NewLoco.GCommon.Enums;
using static NewLoco.GCommon.EntityValidationConstants.Locomotive;
using static NewLoco.GCommon.EntityValidationConstants.BaseEntity;

namespace NewLoco.Web.ViewModels.Locomotives;

public class LocomotiveFormModel
{
    [Required(
        
        ErrorMessageResourceName = "Error_Number_Required" // required
    )]
    [StringLength(
       LocomotiveNumberLength, ErrorMessageResourceName = "Error_Number_Length" // must be exactly 6 chars (NN-NNN)
    )]
    [RegularExpression(
        @"^[0-9]{2}\-[0-9]{3}$",
     
        ErrorMessageResourceName = "Error_Number_Format" // enforce NN-NNN pattern
    )]
    public string Number { get; set; } = null!;

    [StringLength(
        NoteMaxLength,
        MinimumLength = NoteMinLength,
     
        ErrorMessageResourceName = "Error_Note_Length" // min/max length
    )]
    public string? Note { get; set; }

    // default type for shunters
    public LocomotiveType LocomotiveType { get; set; } = LocomotiveType.Shunter;

    // default measuring unit
    public MeasuringUnits MeasuringUnit { get; set; } = MeasuringUnits.Mh;
}