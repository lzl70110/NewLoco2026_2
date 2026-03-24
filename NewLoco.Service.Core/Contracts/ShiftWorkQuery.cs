using System;

namespace NewLoco.Service.Core.Contracts
{
    // Filtering + paging for the Shift Works list
    public sealed class ShiftWorkQuery
    {
        public string? LocomotiveNumber { get; init; }  
        public DateOnly? From { get; init; }           // inclusive (00:00 of the day)
        public DateOnly? To { get; init; }             // inclusive (23:59:59.999... via < next day)

        // With global HasQueryFilter(e => !e.IsDeleted) enabled in LocoDbContext,
        // deleted rows are hidden by default. Admin can opt-in to include them.
        public bool IncludeDeleted { get; init; } = false;

        public int Page { get; init; } = 1;              
        public int PageSize { get; init; } = 20;         
    }
}
