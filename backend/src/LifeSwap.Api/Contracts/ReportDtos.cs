using LifeSwap.Api.Domain;

namespace LifeSwap.Api.Contracts;

public sealed class ReportSummaryDto
{
    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public RequestType? RequestType { get; init; }

    public string? Department { get; init; }

    public int TotalRequests { get; init; }

    public int SubmittedCount { get; init; }

    public int ApprovedCount { get; init; }

    public int RejectedCount { get; init; }

    public int CancelledCount { get; init; }

    public double ApprovedOvertimeHours { get; init; }

    public double ApprovalRate { get; init; }
}

public sealed class TrendPointDto
{
    public DateOnly Date { get; init; }

    public int TotalRequests { get; init; }

    public int ApprovedCount { get; init; }

    public int RejectedCount { get; init; }

    public int CancelledCount { get; init; }

    public double ApprovedOvertimeHours { get; init; }
}

public sealed class ComplianceWarningDto
{
    public string EmployeeId { get; init; } = string.Empty;

    public int Year { get; init; }

    public int Month { get; init; }

    public double ApprovedOvertimeHours { get; init; }

    public double MonthlyOvertimeHourLimit { get; init; }

    public string Severity { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;
}
