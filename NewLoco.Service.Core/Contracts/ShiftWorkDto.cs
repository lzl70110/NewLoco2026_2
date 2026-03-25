using System;
using NewLoco.GCommon.Enums;

namespace NewLoco.Service.Core.Contracts
{
    // DTO used by list/search/paging and "last shift" helpers
    public class ShiftWorkDto
    {
        public int Id { get; set; }

        // Locomotive linkage
        public int LocoId { get; set; }
        public string LocomotiveNumber { get; set; } = string.Empty;

        // When + which shift
        public DateTime Date { get; set; }
        public Shift Shift { get; set; }

        // Who entered it (maps from BaseEntity.CreatedBy)
        public string Operator { get; set; } = string.Empty;

        // Values
        public decimal InitialValue { get; set; }
        public decimal FinalValue { get; set; }
        public decimal Amount { get; set; }

        // Meta
        public string? Note { get; set; }
        public bool IsDeleted { get; set; }
    }

    // change: add create DTO used by service/controller
    public record ShiftWorkCreateDto(
        int LocomotiveId,
        DateTime Date,
        Shift Shift,
        decimal InitialValue,
        decimal FinalValue,
        string? Note
    );
}