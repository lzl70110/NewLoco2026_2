using System;
using NewLoco.GCommon.Enums;

namespace NewLoco.Data.Models
{
    public class ShiftWork : BaseEntity
    {
        public int Id { get; set; }
        public int LocomotiveId { get; set; }
        public Locomotive? Locomotive { get; set; }
        public DateTime Date { get; set; }
        public Shift Shift { get; set; }
        public decimal InitialValue { get; set; }
        public decimal FinalValue { get; set; }
        public decimal Amount { get; set; }
        public bool IsWorkDay { get; set; }


    }
}