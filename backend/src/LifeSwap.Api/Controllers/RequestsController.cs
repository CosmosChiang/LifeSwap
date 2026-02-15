using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using LifeSwap.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var query = dbContext.TimeOffRequests.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(employeeId))
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
        if (string.IsNullOrWhiteSpace(input.EmployeeId) || string.IsNullOrWhiteSpace(input.Reason))
        {
            return BadRequest("EmployeeId and Reason are required.");
        }

        if (input.RequestType is RequestType.Overtime && (input.StartTime is null || input.EndTime is null))
        {
            return BadRequest("Overtime request requires start and end time.");
        }

        var request = new TimeOffRequest
        {
            RequestType = input.RequestType,
            EmployeeId = input.EmployeeId.Trim(),
            DepartmentCode = string.IsNullOrWhiteSpace(input.DepartmentCode) ? "UNASSIGNED" : input.DepartmentCode.Trim(),
            RequestDate = input.RequestDate,
            StartTime = input.StartTime,
            EndTime = input.EndTime,
            Reason = input.Reason.Trim(),
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
            return BadRequest("Only draft requests can be submitted.");
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