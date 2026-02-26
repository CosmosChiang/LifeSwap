using LifeSwap.Api.Contracts;

namespace LifeSwap.Api.Services;

public interface IUserManagementService
{
    Task<List<UserDto>> GetAllUsersAsync();
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<UserDto> CreateUserAsync(CreateUserDto dto);
    Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserDto dto);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> AssignRolesAsync(Guid userId, List<Guid> roleIds);
    Task<bool> ResetPasswordAsync(Guid userId, string newPassword);
}
