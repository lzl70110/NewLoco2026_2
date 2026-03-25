using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace NewLoco.Web.ViewModels.ShiftWorks;

public class CreateShiftWorkViewModel : ShiftWorksViewModelBase
{
    [BindNever] // Do not bind from POST (computed on server)
    public decimal TotalValue => FinalValue - InitialValue;

    public DateTime? InitialValueDate { get; set; }
}