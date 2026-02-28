namespace LifeSwap.Api.Contracts;

public sealed class AutomationWorkflowStatusDto
{
    public string Name { get; init; } = string.Empty;

    public DateTimeOffset? LastStartedAt { get; init; }

    public DateTimeOffset? LastCompletedAt { get; init; }

    public bool LastSucceeded { get; init; }

    public int LastAttemptCount { get; init; }

    public int ConsecutiveFailures { get; init; }

    public string? LastError { get; init; }
}

public sealed class AutomationStatusResponseDto
{
    public bool SchedulerEnabled { get; init; }

    public int ReminderIntervalMinutes { get; init; }

    public int ReportIntervalMinutes { get; init; }

    public int MaxRetryCount { get; init; }

    public IReadOnlyCollection<AutomationWorkflowStatusDto> Workflows { get; init; } = [];
}

public sealed class AutomationSchedulerStateRequestDto
{
    public bool Enabled { get; init; }
}
