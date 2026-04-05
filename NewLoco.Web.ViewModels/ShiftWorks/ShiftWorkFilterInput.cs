using GCommon.Enums;

public sealed record ShiftWorkFilterInput
{
    public string? LocomotiveNumber { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
    public ShiftWorkShowMode ShowMode { get; init; } = ShiftWorkShowMode.Active;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}