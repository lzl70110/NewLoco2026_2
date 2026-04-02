using NewLoco.GCommon.Enums;

namespace NewLoco.Service.Core.Contracts;

// List row DTO (5 args)
public record LocoNumberDto(
    int Id,
    string Number,
    LocomotiveType LocomotiveType,
    MeasuringUnits MeasuringUnit,
    int AxlesCount,
    bool IsDeleted,
    string? Note);

// Details DTO (10 args)
public record LocoDetailsDto(
    int Id,
    string Number,
    LocomotiveType LocomotiveType,
    MeasuringUnits MeasuringUnit,
    int AxlesCount,
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
    int AxlesCount,
    string? Note);

// Dropdown DTO (3 args)
public record LocoOptionDto(int Id, string Number, int AxlesCount);