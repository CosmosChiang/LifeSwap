using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Services;

public class RoleManagementService : IRoleManagementService
{
    private readonly AppDbContext _dbContext;

    public RoleManagementService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<RoleDto>> GetAllRolesAsync()
    {
        var roles = await _dbContext.Roles.ToListAsync();
        return roles.Select(ToRoleDto).ToList();
    }

    public async Task<RoleDto?> GetRoleByIdAsync(Guid id)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        return role == null ? null : ToRoleDto(role);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleDto dto)
    {
        var role = new Role
        {
            Name = dto.Name,
            Description = dto.Description
        };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();
        return ToRoleDto(role);
    }

    public async Task<RoleDto?> UpdateRoleAsync(Guid id, UpdateRoleDto dto)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        if (role == null) return null;
        role.Name = dto.Name;
        role.Description = dto.Description;
        await _dbContext.SaveChangesAsync();
        return ToRoleDto(role);
    }

    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        if (role == null) return false;
        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static RoleDto ToRoleDto(Role role)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description
        };
    }
}
