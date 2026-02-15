using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController(AppDbContext dbContext) : ControllerBase
{
    /// <summary>
    /// Gets summary metrics for requests by period, type, and department prefix.
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<ReportSummaryDto>> GetSummaryAsync(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] RequestType? requestType,
        [FromQuery] string? department,
        CancellationToken cancellationToken)
    {
        var (rangeStart, rangeEnd) = ResolveRange(startDate, endDate);
        var requests = await BuildFilteredQuery(rangeStart, rangeEnd, requestType, department)
            .ToListAsync(cancellationToken);

        var approvedOvertimeHours = requests
            .Where(request => request.Status == RequestStatus.Approved && request.RequestType == RequestType.Overtime)
            .Sum(CalculateHours);

        var approvedCount = requests.Count(request => request.Status == RequestStatus.Approved);
        var totalRequests = requests.Count;

        var summary = new ReportSummaryDto
        {
            StartDate = rangeStart,
            EndDate = rangeEnd,
            RequestType = requestType,
            Department = department,
            TotalRequests = totalRequests,
            SubmittedCount = requests.Count(request => request.Status == RequestStatus.Submitted),
            ApprovedCount = approvedCount,
            RejectedCount = requests.Count(request => request.Status == RequestStatus.Rejected),
            CancelledCount = requests.Count(request => request.Status == RequestStatus.Cancelled),
            ApprovedOvertimeHours = Math.Round(approvedOvertimeHours, 2),
            ApprovalRate = totalRequests == 0 ? 0 : Math.Round((double)approvedCount / totalRequests, 4),
        };

        return Ok(summary);
    }

    /// <summary>
    /// Gets daily trend points for requests in the selected period.
    /// </summary>
    [HttpGet("trends")]
    public async Task<ActionResult<IReadOnlyCollection<TrendPointDto>>> GetTrendsAsync(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] RequestType? requestType,
        [FromQuery] string? department,
        CancellationToken cancellationToken)
    {
        var (rangeStart, rangeEnd) = ResolveRange(startDate, endDate);
        var requests = await BuildFilteredQuery(rangeStart, rangeEnd, requestType, department)
            .ToListAsync(cancellationToken);

        var trend = requests
            .GroupBy(request => request.RequestDate)
            .OrderBy(group => group.Key)
            .Select(group => new TrendPointDto
            {
                Date = group.Key,
                TotalRequests = group.Count(),
                ApprovedCount = group.Count(request => request.Status == RequestStatus.Approved),
                RejectedCount = group.Count(request => request.Status == RequestStatus.Rejected),
                CancelledCount = group.Count(request => request.Status == RequestStatus.Cancelled),
                ApprovedOvertimeHours = Math.Round(group
                    .Where(request => request.Status == RequestStatus.Approved && request.RequestType == RequestType.Overtime)
                    .Sum(CalculateHours), 2),
            })
            .ToList();

        return Ok(trend);
    }

    /// <summary>
    /// Gets compliance warnings by employee and month based on overtime-hour threshold.
    /// </summary>
    [HttpGet("compliance-warnings")]
    public async Task<ActionResult<IReadOnlyCollection<ComplianceWarningDto>>> GetComplianceWarningsAsync(
        [FromQuery] DateOnly? startDate,
        [FromQuery] DateOnly? endDate,
        [FromQuery] double monthlyOvertimeHourLimit = 46,
        [FromQuery] string? department = null,
        CancellationToken cancellationToken = default)
    {
        if (monthlyOvertimeHourLimit <= 0)
        {
            return BadRequest("monthlyOvertimeHourLimit must be greater than zero.");
        }

        var (rangeStart, rangeEnd) = ResolveRange(startDate, endDate);

        var approvedOvertimeRequests = await BuildFilteredQuery(rangeStart, rangeEnd, RequestType.Overtime, department)
            .Where(request => request.Status == RequestStatus.Approved)
            .ToListAsync(cancellationToken);

        var warnings = approvedOvertimeRequests
            .GroupBy(request => new
            {
                request.EmployeeId,
                request.RequestDate.Year,
                request.RequestDate.Month,
            })
            .Select(group => new
            {
                group.Key.EmployeeId,
                group.Key.Year,
                group.Key.Month,
                Hours = Math.Round(group.Sum(CalculateHours), 2),
            })
            .Where(entry => entry.Hours >= monthlyOvertimeHourLimit * 0.8)
            .OrderByDescending(entry => entry.Hours)
            .Select(entry => new ComplianceWarningDto
            {
                EmployeeId = entry.EmployeeId,
                Year = entry.Year,
                Month = entry.Month,
                ApprovedOvertimeHours = entry.Hours,
                MonthlyOvertimeHourLimit = monthlyOvertimeHourLimit,
                Severity = entry.Hours >= monthlyOvertimeHourLimit ? "Critical" : "Warning",
                Message = entry.Hours >= monthlyOvertimeHourLimit
                    ? "已超過月加班上限，請立即檢視排班與補休安排。"
                    : "接近月加班上限，建議提前調整人力與排程。",
            })
            .ToList();

        return Ok(warnings);
    }

    /// <summary>
    /// Resolves a valid date range for report queries.
    /// </summary>
    private static (DateOnly StartDate, DateOnly EndDate) ResolveRange(DateOnly? startDate, DateOnly? endDate)
    {
        var end = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = startDate ?? end.AddDays(-30);

        return start > end ? (end, start) : (start, end);
    }

    /// <summary>
    /// Builds base query with shared report filters.
    /// </summary>
    private IQueryable<TimeOffRequest> BuildFilteredQuery(
        DateOnly startDate,
        DateOnly endDate,
        RequestType? requestType,
        string? department)
    {
        var query = dbContext.TimeOffRequests
            .AsNoTracking()
            .Where(request => request.RequestDate >= startDate && request.RequestDate <= endDate);

        if (requestType is not null)
        {
            query = query.Where(request => request.RequestType == requestType);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            var normalizedDepartment = department.Trim();
            query = query.Where(request => request.DepartmentCode == normalizedDepartment);
        }

        return query;
    }

    /// <summary>
    /// Calculates request duration in hours.
    /// </summary>
    private static double CalculateHours(TimeOffRequest request)
    {
        if (request.StartTime is null || request.EndTime is null)
        {
            return 0;
        }

        var duration = request.EndTime.Value.ToTimeSpan() - request.StartTime.Value.ToTimeSpan();

        if (duration <= TimeSpan.Zero)
        {
            return 0;
        }

        return duration.TotalHours;
    }
}
