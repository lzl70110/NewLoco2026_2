using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewLoco.GCommon.Enums;

namespace NewLoco.Web.ViewModels.Locomotives;
public class LocomotiveNumberViewModel
    {
    public int Id { get; set; }
    public string Number { get; set; } = null!;
    public LocomotiveType LocomotiveType { get; set; }
    public MeasuringUnits MeasuringUnit { get; set; }
    public bool IsDeleted { get; set; } = false;

    public string Note { get; set; } = null!;
    }
