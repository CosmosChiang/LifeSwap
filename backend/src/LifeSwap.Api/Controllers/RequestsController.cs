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
[Authorize]
public sealed class RequestsController(
    AppDbContext dbContext,
    IRequestWorkflowService workflowService,
    ITeamsNotificationService teamsNotificationService) : ControllerBase
{
    /// <summary>
    /// Gets all requests with optional employee and status filtering.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TimeOffRequest>>> GetAllAsync(
        [FromQuery] string? employeeId,
        [FromQuery] RequestStatus? status,
        CancellationToken cancellationToken)
    {
        var currentEmployeeId = User.FindFirstValue("EmployeeId");
        var currentRoles = User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var canViewAllRequests =
            currentRoles.Contains("Administrator") ||
            currentRoles.Contains("Manager");

        var query = dbContext.TimeOffRequests.AsNoTracking().AsQueryable();

        if (!canViewAllRequests)
        {
            if (string.IsNullOrWhiteSpace(currentEmployeeId))
            {
                return Forbid();
            }

            if (!string.IsNullOrWhiteSpace(employeeId) &&
                !string.Equals(employeeId, currentEmployeeId, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            query = query.Where(request => request.EmployeeId == currentEmployeeId);
        }
        else if (!string.IsNullOrWhiteSpace(employeeId))
        {
            query = query.Where(request => request.EmployeeId == employeeId);
        }

        if (status is not null)
        {
            query = query.Where(request => request.Status == status);
        }

        var result = await query.ToListAsync(cancellationToken);
        result = result
            .OrderByDescending(request => request.CreatedAt)
            .ToList();

        return Ok(result);
    }

    /// <summary>
    /// Gets a request by identifier.
    /// </summary>
    [HttpGet("{requestId:guid}", Name = "GetRequestById")]
    public async Task<ActionResult<TimeOffRequest>> GetByIdAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var request = await dbContext.TimeOffRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(entity => entity.Id == requestId, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        return Ok(request);
    }

    /// <summary>
    /// Creates a draft request.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TimeOffRequest>> CreateAsync(
        [FromBody] CreateRequestDto input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.EmployeeId) ||
            string.IsNullOrWhiteSpace(input.OvertimeProject) ||
            string.IsNullOrWhiteSpace(input.OvertimeContent) ||
            string.IsNullOrWhiteSpace(input.OvertimeReason))
        {
            return BadRequest("EmployeeId, OvertimeProject, OvertimeContent, and OvertimeReason are required.");
        }

        if (input.OvertimeEndAt <= input.OvertimeStartAt)
        {
            return BadRequest("OvertimeEndAt must be later than OvertimeStartAt.");
        }

        var normalizedEmployeeId = input.EmployeeId.Trim();
        var applicant = await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.EmployeeId == normalizedEmployeeId, cancellationToken);

        var normalizedStartAt = input.OvertimeStartAt;
        var normalizedEndAt = input.OvertimeEndAt;
        var calculatedCompHours = Math.Round((normalizedEndAt - normalizedStartAt).TotalHours, 2);

        if (calculatedCompHours <= 0)
        {
            return BadRequest("Calculated comp time hours must be greater than zero.");
        }

        var request = new TimeOffRequest
        {
            RequestType = RequestType.Overtime,
            EmployeeId = normalizedEmployeeId,
            ApplicantName = applicant?.Username ?? normalizedEmployeeId,
            DepartmentCode = applicant?.DepartmentCode ?? "UNASSIGNED",
            RequestDate = DateOnly.FromDateTime(normalizedStartAt.DateTime),
            StartTime = TimeOnly.FromTimeSpan(normalizedStartAt.TimeOfDay),
            EndTime = TimeOnly.FromTimeSpan(normalizedEndAt.TimeOfDay),
            OvertimeStartAt = normalizedStartAt,
            OvertimeEndAt = normalizedEndAt,
            CompTimeHours = calculatedCompHours,
            OvertimeProject = input.OvertimeProject.Trim(),
            OvertimeContent = input.OvertimeContent.Trim(),
            OvertimeReason = input.OvertimeReason.Trim(),
            Reason = input.OvertimeReason.Trim(),
        };

        dbContext.TimeOffRequests.Add(request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtRoute("GetRequestById", new { requestId = request.Id }, request);
    }

    /// <summary>
    /// Submits a draft request for manager review.
    /// </summary>
    [HttpPost("{requestId:guid}/submit")]
    public async Task<ActionResult<TimeOffRequest>> SubmitAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var request = await dbContext.TimeOffRequests.FirstOrDefaultAsync(entity => entity.Id == requestId, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        if (!workflowService.CanSubmit(request))
        {
            return BadRequest("Only draft or returned requests can be submitted.");
        }

        request.Status = RequestStatus.Submitted;
        request.SubmittedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await teamsNotificationService.SendRequestStatusChangedAsync(request, "Submitted", cancellationToken);
        return Ok(request);
    }

    /// <summary>
    /// Approves a submitted request.
    /// </summary>
    [Authorize(Roles = "Manager,Administrator")]
    [HttpPost("{requestId:guid}/approve")]
    public async Task<ActionResult<TimeOffRequest>> ApproveAsync(
        Guid requestId,
        [FromBody] ReviewRequestDto input,
        CancellationToken cancellationToken)
    {
        var request = await dbContext.TimeOffRequests.FirstOrDefaultAsync(entity => entity.Id == requestId, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        if (!workflowService.CanApprove(request))
        {
            return BadRequest("Only submitted requests can be approved.");
        }

        request.Status = RequestStatus.Approved;
        request.ReviewerId = input.ReviewerId;
        request.ReviewComment = input.Comment;
        request.ReviewedAt = DateTimeOffset.UtcNow;

        dbContext.Notifications.Add(new AppNotification
        {
            RecipientEmployeeId = request.EmployeeId,
            Title = "申請已核准",
            Message = $"你的申請 {request.Id} 已核准。",
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await teamsNotificationService.SendRequestStatusChangedAsync(request, "Approved", cancellationToken);
        return Ok(request);
    }

    /// <summary>
    /// Rejects a submitted request.
    /// </summary>
    [Authorize(Roles = "Manager,Administrator")]
    [HttpPost("{requestId:guid}/reject")]
    public async Task<ActionResult<TimeOffRequest>> RejectAsync(
        Guid requestId,
        [FromBody] ReviewRequestDto input,
        CancellationToken cancellationToken)
    {
        var request = await dbContext.TimeOffRequests.FirstOrDefaultAsync(entity => entity.Id == requestId, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        if (!workflowService.CanReject(request))
        {
            return BadRequest("Only submitted requests can be rejected.");
        }

        request.Status = RequestStatus.Rejected;
        request.ReviewerId = input.ReviewerId;
        request.ReviewComment = input.Comment;
        request.ReviewedAt = DateTimeOffset.UtcNow;

        dbContext.Notifications.Add(new AppNotification
        {
            RecipientEmployeeId = request.EmployeeId,
            Title = "申請已拒絕",
            Message = $"你的申請 {request.Id} 已被拒絕。",
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await teamsNotificationService.SendRequestStatusChangedAsync(request, "Rejected", cancellationToken);
        return Ok(request);
    }

    /// <summary>
    /// Returns a submitted request to the applicant for revision.
    /// </summary>
    [Authorize(Roles = "Manager,Administrator")]
    [HttpPost("{requestId:guid}/return")]
    public async Task<ActionResult<TimeOffRequest>> ReturnAsync(
        Guid requestId,
        [FromBody] ReviewRequestDto input,
        CancellationToken cancellationToken)
    {
        var request = await dbContext.TimeOffRequests.FirstOrDefaultAsync(entity => entity.Id == requestId, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        if (!workflowService.CanReturn(request))
        {
            return BadRequest("Only submitted requests can be returned.");
        }

        request.Status = RequestStatus.Returned;
        request.ReviewerId = input.ReviewerId;
        request.ReviewComment = input.Comment;
        request.ReviewedAt = DateTimeOffset.UtcNow;

        dbContext.Notifications.Add(new AppNotification
        {
            RecipientEmployeeId = request.EmployeeId,
            Title = "申請已退回",
            Message = $"你的申請 {request.Id} 已被退回，請修正後重新送審。",
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await teamsNotificationService.SendRequestStatusChangedAsync(request, "Returned", cancellationToken);
        return Ok(request);
    }

    /// <summary>
    /// Cancels a draft or submitted request.
    /// </summary>
    [HttpPost("{requestId:guid}/cancel")]
    public async Task<ActionResult<TimeOffRequest>> CancelAsync(Guid requestId, CancellationToken cancellationToken)
    {
        var request = await dbContext.TimeOffRequests.FirstOrDefaultAsync(entity => entity.Id == requestId, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        if (!workflowService.CanCancel(request))
        {
            return BadRequest("Only draft or submitted requests can be cancelled.");
        }

        request.Status = RequestStatus.Cancelled;
        request.CancelledAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        await teamsNotificationService.SendRequestStatusChangedAsync(request, "Cancelled", cancellationToken);
        return Ok(request);
    }
}