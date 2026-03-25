namespace NewLoco.Service.Core.Contracts;
public sealed class ShiftWorkQuery
{
    public string? LocomotiveNumber { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    public bool IncludeDeleted { get; init; } = false;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}