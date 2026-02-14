namespace LifeSwap.Api.Domain;

public sealed class TimeOffRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public RequestType RequestType { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

    public DateOnly RequestDate { get; set; }

    public TimeOnly? StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    public string Reason { get; set; } = string.Empty;

    public RequestStatus Status { get; set; } = RequestStatus.Draft;

    public string? ReviewerId { get; set; }

    public string? ReviewComment { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? SubmittedAt { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }

    public DateTimeOffset? CancelledAt { get; set; }
}