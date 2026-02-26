using LifeSwap.Api.Contracts;

namespace LifeSwap.Api.Services;

public interface IRoleManagementService
{
    Task<List<RoleDto>> GetAllRolesAsync();
    Task<RoleDto?> GetRoleByIdAsync(Guid id);
    Task<RoleDto> CreateRoleAsync(CreateRoleDto dto);
    Task<RoleDto?> UpdateRoleAsync(Guid id, UpdateRoleDto dto);
    Task<bool> DeleteRoleAsync(Guid id);
}
