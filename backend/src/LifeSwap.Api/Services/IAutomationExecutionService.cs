namespace LifeSwap.Api.Services;

public interface IAutomationExecutionService
{
    /// <summary>
    /// Runs the reminder workflow once with retry and status tracking.
    /// </summary>
    Task RunReminderAsync(string trigger, CancellationToken cancellationToken);

    /// <summary>
    /// Runs the periodic report workflow once with retry and status tracking.
    /// </summary>
    Task RunReportAsync(string trigger, CancellationToken cancellationToken);
}
