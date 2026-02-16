using System;
using NewLoco.Data.Models;
using NewLoco.GCommon.Enums;

namespace NewLoco.Web.ViewModels.ShiftWorks
    {
    public class ShiftWorksViewModel
        {
        public int Id { get; set; }

        public int LocomotiveId { get; set; }

        public  string Locomotive { get; set; }=null!;
        public DateTime Date { get; set; }
        public decimal StartValue { get; set; }
        public decimal EndValue { get; set; }

        public decimal TotalValue => EndValue - StartValue;


        public Shift Shift { get; set; }

        public string CreatedBy { get; set; } = "System";
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public string? Note { get; set; }

        public bool IsDeleted { get; set; } = false;
        }
    }