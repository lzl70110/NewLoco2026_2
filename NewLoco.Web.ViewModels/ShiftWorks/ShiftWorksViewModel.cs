using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewLoco.Web.ViewModels.ShiftWorks;
internal class ShiftWorksViewModel
    {
    public int Id { get; set; }
 
    public int LocoId { get; set; }
     
    public DateTime Date { get; set; }
 
    public decimal InitialValue { get; set; }
 
    public decimal FinalValue { get; set; }
 
    public decimal Amount { get; set; }
    }
