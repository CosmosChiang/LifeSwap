using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Services;

public sealed class AutomationSchedulerHostedService(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<AutomationOptions> options,
    ILogger<AutomationSchedulerHostedService> logger) : BackgroundService
{
    /// <summary>
    /// Executes scheduler loops for reminder and periodic report workflows.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Automation scheduler is disabled by configuration.");
            return;
        }

        var reminderTask = RunReminderLoopAsync(stoppingToken);
        var reportTask = RunReportLoopAsync(stoppingToken);

        await Task.WhenAll(reminderTask, reportTask);
    }

    /// <summary>
    /// Runs reminder workflow on configured interval.
    /// </summary>
    private async Task RunReminderLoopAsync(CancellationToken stoppingToken)
    {
        await RunWithScopeAsync(service => service.RunReminderOnceAsync(stoppingToken), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(options.Value.ReminderIntervalMinutes, 1)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunWithScopeAsync(service => service.RunReminderOnceAsync(stoppingToken), stoppingToken);
        }
    }

    /// <summary>
    /// Runs periodic report workflow on configured interval.
    /// </summary>
    private async Task RunReportLoopAsync(CancellationToken stoppingToken)
    {
        await RunWithScopeAsync(service => service.RunPeriodicReportOnceAsync(stoppingToken), stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(options.Value.ReportIntervalMinutes, 1)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunWithScopeAsync(service => service.RunPeriodicReportOnceAsync(stoppingToken), stoppingToken);
        }
    }

    /// <summary>
    /// Runs a workflow operation in a DI scope with exception handling.
    /// </summary>
    private async Task RunWithScopeAsync(
        Func<IAutomationWorkflowService, Task> operation,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAutomationWorkflowService>();
            await operation(service);
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
