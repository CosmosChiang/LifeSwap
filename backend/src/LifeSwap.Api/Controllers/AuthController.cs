using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using LifeSwap.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(
    AppDbContext dbContext,
    IPasswordHashService passwordHashService,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns a JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> LoginAsync(
        [FromBody] LoginRequestDto input,
        CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == input.Username, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Unauthorized("Invalid username or password.");
        }

        if (!passwordHashService.VerifyPassword(input.Password, user.PasswordHash))
        {
            return Unauthorized("Invalid username or password.");
        }

        var roles = user.UserRoles.Select(ur => ur.Role).ToList();
        var token = jwtTokenService.GenerateToken(user, roles);

        // Update last login time
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new LoginResponseDto(
            token,
            user.Username,
            user.EmployeeId,
            user.Email,
            user.DepartmentCode,
            roles.Select(r => r.Name).ToList()));
    }
}
