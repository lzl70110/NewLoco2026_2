using NewLoco.GCommon.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using NewLoco.Data.Models;

public class ShiftWorksViewModelBase
    {
    public int LocomotiveId { get; set; }
    
 
    public decimal InitialValue { get; set; }
    public decimal FinalValue { get; set; }

    public DateTime Date { get; set; }

    public Shift Shift { get; set; }

    public string? Note { get; set; }

    public List<SelectListItem> Locomotives { get; set; }
        = new();
    }
