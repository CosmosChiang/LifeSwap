namespace LifeSwap.Api.Domain;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string EmployeeId { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public string DepartmentCode { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastLoginAt { get; set; }

    // Navigation property
    public ICollection<UserRole> UserRoles { get; init; } = [];
}
