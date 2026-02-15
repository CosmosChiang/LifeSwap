using LifeSwap.Api.Domain;

namespace LifeSwap.Api.Contracts;

public sealed record CreateRequestDto(
    RequestType RequestType,
    string EmployeeId,
    string? DepartmentCode,
    DateOnly RequestDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    string Reason);

public sealed record ReviewRequestDto(
    string ReviewerId,
    string? Comment);