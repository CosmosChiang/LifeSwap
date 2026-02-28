using System.Collections.Concurrent;
using LifeSwap.Api.Contracts;

namespace LifeSwap.Api.Services;

public interface IAutomationExecutionStatusStore
{
    /// <summary>
    /// Marks a workflow execution as started.
    /// </summary>
    void MarkStarted(string workflowName);

    /// <summary>
    /// Marks a workflow execution as completed.
    /// </summary>
    void MarkCompleted(string workflowName, bool succeeded, int attemptCount, string? error);

    /// <summary>
    /// Returns the latest status snapshot for all tracked workflows.
    /// </summary>
    IReadOnlyCollection<AutomationWorkflowStatusDto> GetAll();
}

public sealed class AutomationExecutionStatusStore : IAutomationExecutionStatusStore
{
    private readonly ConcurrentDictionary<string, StatusState> stateByWorkflow = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Marks a workflow execution as started.
    /// </summary>
    public void MarkStarted(string workflowName)
    {
        var now = DateTimeOffset.UtcNow;
        stateByWorkflow.AddOrUpdate(
            workflowName,
            _ => new StatusState
            {
                Name = workflowName,
                LastStartedAt = now,
                LastCompletedAt = null,
                LastSucceeded = false,
                LastAttemptCount = 0,
                ConsecutiveFailures = 0,
                LastError = null,
            },
            (_, current) => current with
            {
                LastStartedAt = now,
                LastCompletedAt = null,
            });
    }

    /// <summary>
    /// Marks a workflow execution as completed.
    /// </summary>
    public void MarkCompleted(string workflowName, bool succeeded, int attemptCount, string? error)
    {
        var now = DateTimeOffset.UtcNow;
        stateByWorkflow.AddOrUpdate(
            workflowName,
            _ => new StatusState
            {
                Name = workflowName,
                LastStartedAt = null,
                LastCompletedAt = now,
                LastSucceeded = succeeded,
                LastAttemptCount = attemptCount,
                ConsecutiveFailures = succeeded ? 0 : 1,
                LastError = error,
            },
            (_, current) => current with
            {
                LastCompletedAt = now,
                LastSucceeded = succeeded,
                LastAttemptCount = attemptCount,
                ConsecutiveFailures = succeeded ? 0 : current.ConsecutiveFailures + 1,
                LastError = error,
            });
    }

    /// <summary>
    /// Returns the latest status snapshot for all tracked workflows.
    /// </summary>
    public IReadOnlyCollection<AutomationWorkflowStatusDto> GetAll()
    {
        return stateByWorkflow.Values
            .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
            .Select(item => new AutomationWorkflowStatusDto
            {
                Name = item.Name,
                LastStartedAt = item.LastStartedAt,
                LastCompletedAt = item.LastCompletedAt,
                LastSucceeded = item.LastSucceeded,
                LastAttemptCount = item.LastAttemptCount,
                ConsecutiveFailures = item.ConsecutiveFailures,
                LastError = item.LastError,
            })
            .ToList();
    }

    private sealed record StatusState
    {
        public string Name { get; init; } = string.Empty;

        public DateTimeOffset? LastStartedAt { get; init; }

        public DateTimeOffset? LastCompletedAt { get; init; }

        public bool LastSucceeded { get; init; }

        public int LastAttemptCount { get; init; }

        public int ConsecutiveFailures { get; init; }

        public string? LastError { get; init; }
    }
}
