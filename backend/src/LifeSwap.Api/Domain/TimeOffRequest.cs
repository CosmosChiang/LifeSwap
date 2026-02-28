namespace LifeSwap.Api.Domain;

public sealed class TimeOffRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public RequestType RequestType { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

    public string ApplicantName { get; set; } = string.Empty;

    public string DepartmentCode { get; set; } = string.Empty;

    public DateOnly RequestDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public DateTimeOffset? OvertimeStartAt { get; set; }

    public DateTimeOffset? OvertimeEndAt { get; set; }

    public double CompTimeHours { get; set; }

    public string OvertimeProject { get; set; } = string.Empty;

    public string OvertimeContent { get; set; } = string.Empty;

    public string OvertimeReason { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public RequestStatus Status { get; set; } = RequestStatus.Draft;

    public string? ReviewerId { get; set; }

    public string? ReviewComment { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? SubmittedAt { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }
}