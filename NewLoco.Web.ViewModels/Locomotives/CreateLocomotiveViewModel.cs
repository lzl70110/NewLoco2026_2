using Microsoft.AspNetCore.Mvc.Rendering;
using NewLoco.GCommon.Enums;

namespace NewLoco.Web.ViewModels.Locomotives;

public class CreateLocomotiveViewModel : LocomotiveFormModel
{
    public IEnumerable<SelectListItem> LocomotiveTypes { get; set; } =
        Enum.GetValues(typeof(LocomotiveType))
            .Cast<LocomotiveType>()
            .Select(x => new SelectListItem
            {
                Text = x.ToString(),
                Value = ((int)x).ToString()
            });

    public IEnumerable<SelectListItem> MeasuringUnitsList { get; set; } =
        Enum.GetValues(typeof(MeasuringUnits))
            .Cast<MeasuringUnits>()
            .Select(x => new SelectListItem
            {
                Text = x.ToString(),
                Value = ((int)x).ToString()
            });
}