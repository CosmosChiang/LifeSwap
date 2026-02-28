using LifeSwap.Api.Domain;

namespace LifeSwap.Api.Contracts;

public sealed record CreateRequestDto(
    string EmployeeId,
    DateTimeOffset OvertimeStartAt,
    DateTimeOffset OvertimeEndAt,
    string OvertimeProject,
    string OvertimeContent,
    string OvertimeReason);

public sealed record ReviewRequestDto(
    string ReviewerId,
    string? Comment);