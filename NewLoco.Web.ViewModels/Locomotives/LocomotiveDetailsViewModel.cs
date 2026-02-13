using NewLoco.GCommon.Enums;
using System.Collections.Generic;
using NewLoco.Data.Models;
using NewLoco.Data.Models.Fuel;
 
namespace NewLoco.Web.ViewModels.Locomotives
    {
    public class LocomotiveDetailsViewModel
        {
        public int Id { get; set; }
        public string Number { get; set; }=null!;
        public LocomotiveType LocomotiveType { get; set; }
        public MeasuringUnits MeasuringUnit { get; set; }
        public bool IsDeleted { get; set; }
        public string Note { get; set; }="";
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = "Admin";

        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; } =String.Empty;

        }
    }
