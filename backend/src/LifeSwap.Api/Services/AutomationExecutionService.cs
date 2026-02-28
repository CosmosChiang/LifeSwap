using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Services;

public sealed class AutomationExecutionService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<AutomationOptions> options,
    IAutomationExecutionStatusStore statusStore,
    ILogger<AutomationExecutionService> logger) : IAutomationExecutionService
{
    private static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Runs the reminder workflow once with retry and status tracking.
    /// </summary>
    public Task RunReminderAsync(string trigger, CancellationToken cancellationToken)
    {
        return ExecuteWithRetryAsync(
            workflowName: "Reminder",
            trigger: trigger,
            operation: service => service.RunReminderOnceAsync(cancellationToken),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Runs the periodic report workflow once with retry and status tracking.
    /// </summary>
    public Task RunReportAsync(string trigger, CancellationToken cancellationToken)
    {
        return ExecuteWithRetryAsync(
            workflowName: "PeriodicReport",
            trigger: trigger,
            operation: service => service.RunPeriodicReportOnceAsync(cancellationToken),
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Executes a workflow operation with bounded retries and status updates.
    /// </summary>
    private async Task ExecuteWithRetryAsync(
        string workflowName,
        string trigger,
        Func<IAutomationWorkflowService, Task> operation,
        CancellationToken cancellationToken)
    {
        statusStore.MarkStarted(workflowName);

        var retryCount = Math.Max(options.Value.MaxRetryCount, 0);
        var retryDelay = TimeSpan.FromSeconds(Math.Max(options.Value.RetryDelaySeconds, 1));

        Exception? lastException = null;

        for (var attempt = 1; attempt <= retryCount + 1; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IAutomationWorkflowService>();
                await operation(service);

                statusStore.MarkCompleted(workflowName, succeeded: true, attemptCount: attempt, error: null);
                logger.LogInformation(
                    "Automation workflow {WorkflowName} succeeded. Trigger={Trigger}, Attempt={Attempt}",
                    workflowName,
                    trigger,
                    attempt);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                lastException = exception;
                logger.LogWarning(
                    exception,
                    "Automation workflow {WorkflowName} failed. Trigger={Trigger}, Attempt={Attempt}/{MaxAttempt}",
                    workflowName,
                    trigger,
                    attempt,
                    retryCount + 1);

                if (attempt > retryCount)
                {
                    break;
                }

                var delay = retryDelay == TimeSpan.Zero ? DefaultRetryDelay : retryDelay;
                await Task.Delay(delay, cancellationToken);
            }
        }

        statusStore.MarkCompleted(
            workflowName,
            succeeded: false,
            attemptCount: retryCount + 1,
            error: lastException?.Message);

        throw new InvalidOperationException(
            $"Automation workflow {workflowName} failed after {retryCount + 1} attempts.",
            lastException);
    }
}
