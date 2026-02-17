using System;

namespace NewLoco.Web.ViewModels.ShiftWorks
    {
    public class CreateShiftWorkViewModel : ShiftWorksViewModelBase
        {
        
        public decimal TotalValue => FinalValue - InitialValue;

       
        public DateTime? InitialValueDate { get; set; }
        }
    }
