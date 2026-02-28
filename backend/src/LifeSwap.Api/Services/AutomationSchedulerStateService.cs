using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Services;

public interface IAutomationSchedulerStateService
{
    /// <summary>
    /// Gets whether scheduler-triggered automation is enabled.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Updates scheduler enabled state.
    /// </summary>
    void SetEnabled(bool enabled);
}

public sealed class AutomationSchedulerStateService : IAutomationSchedulerStateService
{
    private int enabledFlag;

    public AutomationSchedulerStateService(IOptions<AutomationOptions> options)
    {
        enabledFlag = options.Value.Enabled ? 1 : 0;
    }

    /// <summary>
    /// Gets whether scheduler-triggered automation is enabled.
    /// </summary>
    public bool IsEnabled => Volatile.Read(ref enabledFlag) == 1;

    /// <summary>
    /// Updates scheduler enabled state.
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        Interlocked.Exchange(ref enabledFlag, enabled ? 1 : 0);
    }
}
