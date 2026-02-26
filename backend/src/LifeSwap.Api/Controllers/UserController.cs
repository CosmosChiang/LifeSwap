using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using LifeSwap.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public class UserController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IUserManagementService _userService;

    public UserController(AppDbContext dbContext, IUserManagementService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUser(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser(CreateUserDto dto)
    {
        var user = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, UpdateUserDto dto)
    {
        var user = await _userService.UpdateUserAsync(id, dto);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var result = await _userService.DeleteUserAsync(id);
        if (!result) return NotFound();
        return NoContent();
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRoles(Guid id, AssignRolesDto dto)
    {
        var result = await _userService.AssignRolesAsync(id, dto.RoleIds);
        if (!result) return NotFound();
        return Ok();
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(Guid id, ResetPasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
        {
            return BadRequest("NewPassword must be at least 8 characters.");
        }

        var result = await _userService.ResetPasswordAsync(id, dto.NewPassword);
        if (!result) return NotFound();
        return Ok();
    }
}
