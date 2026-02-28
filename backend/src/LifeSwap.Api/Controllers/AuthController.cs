using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using LifeSwap.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            return this.CreateAuthenticationProblemResponse("Invalid username or password.");
        }

        if (!passwordHashService.VerifyPassword(input.Password, user.PasswordHash))
        {
            return this.CreateAuthenticationProblemResponse("Invalid username or password.");
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

    /// <summary>
    /// Changes current user's password.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequestDto input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.CurrentPassword) || string.IsNullOrWhiteSpace(input.NewPassword))
        {
            return this.CreateValidationProblemResponse(
                "Invalid password update request.",
                "CurrentPassword and NewPassword are required.");
        }

        if (input.NewPassword.Length < 8)
        {
            return this.CreateValidationProblemResponse(
                "Invalid password policy.",
                "NewPassword must be at least 8 characters.");
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return this.CreateAuthenticationProblemResponse("Authentication context is invalid.");
        }

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        if (!passwordHashService.VerifyPassword(input.CurrentPassword, user.PasswordHash))
        {
            return this.CreateValidationProblemResponse(
                "Invalid current password.",
                "Current password is incorrect.");
        }

        user.PasswordHash = passwordHashService.HashPassword(input.NewPassword);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok();
    }
}
