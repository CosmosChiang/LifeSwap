namespace LifeSwap.Api.Contracts;

public sealed record LoginRequestDto(
    string Username,
    string Password);

public sealed record LoginResponseDto(
    string Token,
    string Username,
    string EmployeeId,
    string Email,
    string DepartmentCode,
    IReadOnlyCollection<string> Roles);
