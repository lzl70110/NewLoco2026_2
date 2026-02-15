using NewLoco.GCommon.Enums;

namespace NewLoco.Service.Core.Contracts;

// List row DTO (5 args)
public record LocoNumberDto(
    int Id,
    string Number,
    LocomotiveType LocomotiveType,
    MeasuringUnits MeasuringUnit,
    bool IsDeleted);

// Details DTO (10 args)
public record LocoDetailsDto(
    int Id,
    string Number,
    LocomotiveType LocomotiveType,
    MeasuringUnits MeasuringUnit,
    string? Note,
    bool IsDeleted,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? ModifiedOn,
    string? ModifiedBy);

// Form DTO (4 args)
public record LocomotiveFormDto(
    string Number,
    LocomotiveType LocomotiveType,
    MeasuringUnits MeasuringUnit,
    string? Note);

// Dropdown DTO (2 args)
public record LocoOptionDto(int Id, string Number);