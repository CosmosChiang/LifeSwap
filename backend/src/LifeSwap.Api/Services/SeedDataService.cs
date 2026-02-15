using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Services;

public sealed class SeedDataService
{
    public static async Task SeedAsync(AppDbContext dbContext, IPasswordHashService passwordHashService)
    {
        // Ensure roles exist
        var roles = new[]
        {
            new Role { Name = "Employee", Description = "Regular employee" },
            new Role { Name = "Manager", Description = "Department manager" },
            new Role { Name = "HR", Description = "Human Resources" },
            new Role { Name = "Administrator", Description = "System administrator" },
        };

        foreach (var role in roles)
        {
            if (!await dbContext.Roles.AnyAsync(r => r.Name == role.Name))
            {
                dbContext.Roles.Add(role);
            }
        }

        await dbContext.SaveChangesAsync();

        // Seed test users (only in development)
        var testUsers = new[]
        {
            new
            {
                EmployeeId = "E001",
                Username = "employee1",
                Email = "employee1@lifeswap.local",
                Department = "ENG",
                Role = "Employee",
                Password = "Password123!"
            },
            new
            {
                EmployeeId = "M001",
                Username = "manager1",
                Email = "manager1@lifeswap.local",
                Department = "ENG",
                Role = "Manager",
                Password = "Password123!"
            },
            new
            {
                EmployeeId = "HR001",
                Username = "hr_admin",
                Email = "hr@lifeswap.local",
                Department = "HR",
                Role = "HR",
                Password = "Password123!"
            },
            new
            {
                EmployeeId = "ADMIN001",
                Username = "admin",
                Email = "admin@lifeswap.local",
                Department = "IT",
                Role = "Administrator",
                Password = "Password123!"
            },
        };

        foreach (var testUser in testUsers)
        {
            if (!await dbContext.Users.AnyAsync(u => u.Username == testUser.Username))
            {
                var user = new User
                {
                    EmployeeId = testUser.EmployeeId,
                    Username = testUser.Username,
                    Email = testUser.Email,
                    DepartmentCode = testUser.Department,
                    PasswordHash = passwordHashService.HashPassword(testUser.Password),
                    IsActive = true,
                };

                dbContext.Users.Add(user);
                await dbContext.SaveChangesAsync();

                // Assign role
                var role = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == testUser.Role);
                if (role is not null)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id,
                    };
                    dbContext.UserRoles.Add(userRole);
                    await dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
