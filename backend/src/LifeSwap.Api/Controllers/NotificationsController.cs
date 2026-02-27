using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LifeSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class NotificationsController(AppDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Gets notifications for current authenticated user.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotificationItemDto>>> GetMyNotificationsAsync(
        [FromQuery] bool unreadOnly = false,
        CancellationToken cancellationToken = default)
    {
        var employeeId = User.FindFirstValue("EmployeeId");
        if (string.IsNullOrWhiteSpace(employeeId))
        {
            return Forbid();
        }

        var query = dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.RecipientEmployeeId == employeeId);

        if (unreadOnly)
        {
            query = query.Where(notification => !notification.IsRead);
        }

        var notifications = await query
            .Select(notification => new NotificationItemDto
            {
                Id = notification.Id,
                RecipientEmployeeId = notification.RecipientEmployeeId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        notifications = notifications
            .OrderByDescending(notification => notification.CreatedAt)
            .ToList();

        return Ok(notifications);
    }

    /// <summary>
    /// Marks one notification as read for current authenticated user.
    /// </summary>
    [HttpPost("{notificationId:guid}/read")]
    public async Task<IActionResult> MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var employeeId = User.FindFirstValue("EmployeeId");
        if (string.IsNullOrWhiteSpace(employeeId))
        {
            return Forbid();
        }

        var notification = await dbContext.Notifications
            .FirstOrDefaultAsync(
                item => item.Id == notificationId && item.RecipientEmployeeId == employeeId,
                cancellationToken);

        if (notification is null)
        {
            return NotFound();
        }

        if (notification.IsRead)
        {
            return Ok();
        }

        notification.IsRead = true;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok();
    }
}
