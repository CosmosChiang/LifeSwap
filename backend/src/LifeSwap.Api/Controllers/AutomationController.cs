using LifeSwap.Api.Contracts;
using LifeSwap.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Administrator")]
public sealed class AutomationController(
    IAutomationExecutionService executionService,
    IAutomationSchedulerStateService schedulerState,
    IAutomationExecutionStatusStore statusStore,
    IOptions<AutomationOptions> options,
    ILogger<AutomationController> logger) : ControllerBase
{
    /// <summary>
    /// Gets automation scheduler configuration and latest workflow execution status.
    /// </summary>
    [HttpGet("status")]
    public ActionResult<AutomationStatusResponseDto> GetStatus()
    {
        var snapshot = new AutomationStatusResponseDto
        {
            SchedulerEnabled = schedulerState.IsEnabled,
            ReminderIntervalMinutes = options.Value.ReminderIntervalMinutes,
            ReportIntervalMinutes = options.Value.ReportIntervalMinutes,
            MaxRetryCount = options.Value.MaxRetryCount,
            Workflows = statusStore.GetAll(),
        };

        return Ok(snapshot);
    }

    /// <summary>
    /// Manually runs reminder workflow once.
    /// </summary>
    [HttpPost("run-reminder")]
    public async Task<IActionResult> RunReminderAsync(CancellationToken cancellationToken)
    {
        try
        {
            await executionService.RunReminderAsync("Manual", cancellationToken);
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Manual reminder workflow run failed.");
            return this.CreateInternalProblemResponse(
                "Automation workflow failed.",
                "Reminder workflow failed.");
        }
    }

    /// <summary>
    /// Manually runs periodic report workflow once.
    /// </summary>
    [HttpPost("run-report")]
    public async Task<IActionResult> RunReportAsync(CancellationToken cancellationToken)
    {
        try
        {
            await executionService.RunReportAsync("Manual", cancellationToken);
            return Ok();
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Manual report workflow run failed.");
            return this.CreateInternalProblemResponse(
                "Automation workflow failed.",
                "Report workflow failed.");
        }
    }

    /// <summary>
    /// Updates scheduler enabled state for runtime automation loops.
    /// </summary>
    [HttpPost("scheduler-state")]
    public IActionResult UpdateSchedulerState([FromBody] AutomationSchedulerStateRequestDto input)
    {
        schedulerState.SetEnabled(input.Enabled);
        return Ok();
    }
}
