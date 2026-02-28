using LifeSwap.Api.Controllers;
using LifeSwap.Api.Contracts;
using LifeSwap.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Tests;

public sealed class AutomationControllerTests
{
    [Fact]
    public async Task RunReminderAsync_WhenExecutionFails_ReturnsServerError()
    {
        var executionService = new FakeExecutionService
        {
            ThrowOnReminder = true,
        };

        var controller = new AutomationController(
            executionService,
            new AutomationSchedulerStateService(Options.Create(new AutomationOptions
            {
                Enabled = true,
            })),
            new AutomationExecutionStatusStore(),
            Options.Create(new AutomationOptions()),
            NullLogger<AutomationController>.Instance);

        var result = await controller.RunReminderAsync(CancellationToken.None);
        var status = Assert.IsType<ObjectResult>(result);

        Assert.Equal(StatusCodes.Status500InternalServerError, status.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(status.Value);
        Assert.Equal("Automation workflow failed.", problem.Title);
        Assert.Equal("Reminder workflow failed.", problem.Detail);
    }

    [Fact]
    public void UpdateSchedulerState_UpdatesRuntimeEnabledStatus()
    {
        var state = new AutomationSchedulerStateService(Options.Create(new AutomationOptions
        {
            Enabled = true,
        }));

        var controller = new AutomationController(
            new FakeExecutionService(),
            state,
            new AutomationExecutionStatusStore(),
            Options.Create(new AutomationOptions()),
            NullLogger<AutomationController>.Instance);

        var result = controller.UpdateSchedulerState(new AutomationSchedulerStateRequestDto
        {
            Enabled = false,
        });

        Assert.IsType<OkResult>(result);
        Assert.False(state.IsEnabled);
    }

    [Fact]
    public void GetStatus_ReturnsConfiguredIntervalsAndWorkflows()
    {
        var statusStore = new AutomationExecutionStatusStore();
        statusStore.MarkStarted("Reminder");
        statusStore.MarkCompleted("Reminder", true, 1, null);

        var controller = new AutomationController(
            new FakeExecutionService(),
            new AutomationSchedulerStateService(Options.Create(new AutomationOptions
            {
                Enabled = true,
            })),
            statusStore,
            Options.Create(new AutomationOptions
            {
                Enabled = true,
                ReminderIntervalMinutes = 5,
                ReportIntervalMinutes = 60,
                MaxRetryCount = 2,
            }),
            NullLogger<AutomationController>.Instance);

        var result = controller.GetStatus();
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<AutomationStatusResponseDto>(ok.Value);

        Assert.True(payload.SchedulerEnabled);
        Assert.Equal(5, payload.ReminderIntervalMinutes);
        Assert.Equal(60, payload.ReportIntervalMinutes);
        Assert.Equal(2, payload.MaxRetryCount);
        Assert.Single(payload.Workflows);
        Assert.Equal("Reminder", payload.Workflows.Single().Name);
    }

    private sealed class FakeExecutionService : IAutomationExecutionService
    {
        public bool ThrowOnReminder { get; init; }

        public Task RunReminderAsync(string trigger, CancellationToken cancellationToken)
        {
            if (ThrowOnReminder)
            {
                throw new InvalidOperationException("reminder failed");
            }

            return Task.CompletedTask;
        }

        public Task RunReportAsync(string trigger, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
