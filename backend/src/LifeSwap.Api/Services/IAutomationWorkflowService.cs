namespace LifeSwap.Api.Services;

public interface IAutomationWorkflowService
{
    /// <summary>
    /// Executes pending-review reminder workflow once.
    /// </summary>
    Task RunReminderOnceAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Executes periodic report workflow once.
    /// </summary>
    Task RunPeriodicReportOnceAsync(CancellationToken cancellationToken);
}
