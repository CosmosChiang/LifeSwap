using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Services;

public sealed class AutomationSchedulerHostedService(
    IAutomationExecutionService executionService,
    IAutomationSchedulerStateService schedulerState,
    IOptions<AutomationOptions> options,
    ILogger<AutomationSchedulerHostedService> logger) : BackgroundService
{
    /// <summary>
    /// Executes scheduler loops for reminder and periodic report workflows.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Automation scheduler started. InitialEnabled={Enabled}", schedulerState.IsEnabled);

        var reminderTask = RunReminderLoopAsync(stoppingToken);
        var reportTask = RunReportLoopAsync(stoppingToken);

        await Task.WhenAll(reminderTask, reportTask);
    }

    /// <summary>
    /// Runs reminder workflow on configured interval.
    /// </summary>
    private async Task RunReminderLoopAsync(CancellationToken stoppingToken)
    {
        await RunWorkflowIfEnabledAsync(
            operation: cancellationToken => executionService.RunReminderAsync("Scheduler", cancellationToken),
            workflowName: "Reminder",
            cancellationToken: stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(options.Value.ReminderIntervalMinutes, 1)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunWorkflowIfEnabledAsync(
                operation: cancellationToken => executionService.RunReminderAsync("Scheduler", cancellationToken),
                workflowName: "Reminder",
                cancellationToken: stoppingToken);
        }
    }

    /// <summary>
    /// Runs periodic report workflow on configured interval.
    /// </summary>
    private async Task RunReportLoopAsync(CancellationToken stoppingToken)
    {
        await RunWorkflowIfEnabledAsync(
            operation: cancellationToken => executionService.RunReportAsync("Scheduler", cancellationToken),
            workflowName: "PeriodicReport",
            cancellationToken: stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(options.Value.ReportIntervalMinutes, 1)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunWorkflowIfEnabledAsync(
                operation: cancellationToken => executionService.RunReportAsync("Scheduler", cancellationToken),
                workflowName: "PeriodicReport",
                cancellationToken: stoppingToken);
        }
    }

    /// <summary>
    /// Runs a workflow operation in a DI scope with exception handling.
    /// </summary>
    private async Task RunWorkflowIfEnabledAsync(
        Func<CancellationToken, Task> operation,
        string workflowName,
        CancellationToken cancellationToken)
    {
        if (!schedulerState.IsEnabled)
        {
            logger.LogDebug("Automation workflow skipped because scheduler is disabled. Workflow={WorkflowName}", workflowName);
            return;
        }

        try
        {
            await operation(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Graceful shutdown.
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Automation scheduler operation failed.");
        }
    }
}
