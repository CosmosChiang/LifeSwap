namespace LifeSwap.Api.Contracts;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string DepartmentCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<RoleDto> Roles { get; set; } = new();
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public string Password { get; set; } = string.Empty;
    public List<Guid> RoleIds { get; set; } = new();
}

public class UpdateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string? DepartmentCode { get; set; }
    public bool? IsActive { get; set; }
    public List<Guid>? RoleIds { get; set; }
}

public class ResetPasswordDto
{
    public string NewPassword { get; set; } = string.Empty;
}

public class AssignRolesDto
{
    public List<Guid> RoleIds { get; set; } = new();
}
