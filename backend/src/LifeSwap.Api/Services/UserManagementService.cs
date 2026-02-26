using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Services;

public class UserManagementService : IUserManagementService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHashService _passwordHashService;

    public UserManagementService(AppDbContext dbContext, IPasswordHashService passwordHashService)
    {
        _dbContext = dbContext;
        _passwordHashService = passwordHashService;
    }

    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        var users = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ToListAsync();
        return users.Select(ToUserDto).ToList();
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
        return user == null ? null : ToUserDto(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
    {
        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            EmployeeId = dto.EmployeeId,
            DepartmentCode = string.IsNullOrWhiteSpace(dto.DepartmentCode) ? "N/A" : dto.DepartmentCode,
            PasswordHash = _passwordHashService.HashPassword(dto.Password),
            IsActive = true,
        };
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        // Assign roles
        if (dto.RoleIds.Any())
        {
            foreach (var roleId in dto.RoleIds)
            {
                _dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
            }
            await _dbContext.SaveChangesAsync();
        }
        return await GetUserByIdAsync(user.Id) ?? throw new Exception("User creation failed");
    }

    public async Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserDto dto)
    {
        var user = await _dbContext.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return null;
        user.Email = dto.Email;
        if (!string.IsNullOrWhiteSpace(dto.DepartmentCode))
        {
            user.DepartmentCode = dto.DepartmentCode;
        }
        if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
        await _dbContext.SaveChangesAsync();

        // Update roles if provided
        if (dto.RoleIds != null)
        {
            var currentRoles = user.UserRoles.ToList();
            _dbContext.UserRoles.RemoveRange(currentRoles);
            foreach (var roleId in dto.RoleIds)
            {
                _dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
            }
            await _dbContext.SaveChangesAsync();
        }
        return await GetUserByIdAsync(user.Id);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null) return false;
        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AssignRolesAsync(Guid userId, List<Guid> roleIds)
    {
        var user = await _dbContext.Users.Include(u => u.UserRoles).FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;
        var currentRoles = user.UserRoles.ToList();
        _dbContext.UserRoles.RemoveRange(currentRoles);
        foreach (var roleId in roleIds)
        {
            _dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
        }
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(Guid userId, string newPassword)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.PasswordHash = _passwordHashService.HashPassword(newPassword);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private static UserDto ToUserDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            EmployeeId = user.EmployeeId,
            DepartmentCode = user.DepartmentCode,
            IsActive = user.IsActive,
            Roles = user.UserRoles.Select(ur => new RoleDto
            {
                Id = ur.Role.Id,
                Name = ur.Role.Name,
                Description = ur.Role.Description
            }).ToList()
        };
    }
}
