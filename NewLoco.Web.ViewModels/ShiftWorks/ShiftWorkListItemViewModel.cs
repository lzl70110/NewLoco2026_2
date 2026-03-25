using System;
using NewLoco.GCommon.Enums;

namespace NewLoco.Web.ViewModels.ShiftWorks
{
    public sealed class ShiftWorkListItemViewModel
    {
        public int Id { get; init; }
        public DateTime Date { get; init; }
        public Shift Shift { get; init; }

        public string LocomotiveNumber { get; init; } = string.Empty;
        public string Operator { get; init; } = string.Empty;

        public decimal InitialValue { get; init; }
        public decimal FinalValue { get; init; }
        public decimal Amount { get; init; }

        public string? Note { get; init; }
        public bool IsDeleted { get; init; }
    }
}