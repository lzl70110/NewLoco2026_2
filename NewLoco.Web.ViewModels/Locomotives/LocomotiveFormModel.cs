using System.ComponentModel.DataAnnotations;
using NewLoco.GCommon.Enums;
using static NewLoco.GCommon.EntityValidationConstants.Locomotive;
using static NewLoco.GCommon.EntityValidationConstants.BaseEntity;
using static GCommon.Messages;

namespace NewLoco.Web.ViewModels.Locomotives;

public class LocomotiveFormModel
{
    [Required(
        ErrorMessageResourceType = typeof(Locomotive),
        ErrorMessageResourceName = "Error_Number_Required"
    )]
    [StringLength(
        LocomotiveNumberLength,
        ErrorMessageResourceType = typeof(Locomotive),
        ErrorMessageResourceName = "Error_Number_Length"
    )]
    [RegularExpression(
        @"^[0-9]{2}\-[0-9]{3}$",
        ErrorMessageResourceType = typeof(Locomotive),
        ErrorMessageResourceName = "Error_Number_Format"
    )]
    public string Number { get; set; } = null!;

    [StringLength(
        NoteMaxLength,
        MinimumLength = NoteMinLength,
        ErrorMessageResourceType = typeof(Locomotive),
        ErrorMessageResourceName = "Error_Note_Length"
    )]
    public string? Note { get; set; }

    public LocomotiveType LocomotiveType { get; set; } = LocomotiveType.Shunter;

    public MeasuringUnits MeasuringUnit { get; set; } = MeasuringUnits.Mh;

    [Range(2, 8,
    ErrorMessage = "Axles must be between 2 and 8.")]
    public int AxlesCount { get; set; }
}