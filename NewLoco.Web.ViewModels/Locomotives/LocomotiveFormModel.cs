using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewLoco.GCommon.Enums;
using static NewLoco.GCommon.EntityValidationConstants.BaseEntity;

namespace NewLoco.Web.ViewModels.Locomotives;
public class LocomotiveFormModel
    {
    [Required, StringLength(6)]
    [RegularExpression(@"^[0-9]{2}\-[0-9]{3}$")]
    public string Number { get; set; } = null!;
    [StringLength (NoteMaxLength, MinimumLength = NoteMinLength)]
    public string? Note { get; set; }
    public LocomotiveType LocomotiveType { get; set; } = LocomotiveType.Shunter;
    public MeasuringUnits MeasuringUnit { get; set; } = MeasuringUnits.Mh;
    }