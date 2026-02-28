using LifeSwap.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Tests;

public sealed class AutomationExecutionServiceTests
{
    [Fact]
    public async Task RunReminderAsync_RetriesAndSucceeds_UpdatesStatus()
    {
        var counter = new RetryCounter { FailReminderAttempts = 1 };
        using var provider = BuildProvider(counter);

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var statusStore = new AutomationExecutionStatusStore();
        var options = Options.Create(new AutomationOptions
        {
            MaxRetryCount = 2,
            RetryDelaySeconds = 1,
        });

        var service = new AutomationExecutionService(
            scopeFactory,
            options,
            statusStore,
            NullLogger<AutomationExecutionService>.Instance);

        await service.RunReminderAsync("Manual", CancellationToken.None);

        var status = statusStore.GetAll().Single(item => item.Name == "Reminder");
        Assert.True(status.LastSucceeded);
        Assert.Equal(2, status.LastAttemptCount);
        Assert.Equal(0, status.ConsecutiveFailures);
        Assert.Equal(2, counter.ReminderRuns);
    }

    [Fact]
    public async Task RunReportAsync_WhenExhausted_ThrowsAndUpdatesStatus()
    {
        var counter = new RetryCounter { AlwaysFailReport = true };
        using var provider = BuildProvider(counter);

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var statusStore = new AutomationExecutionStatusStore();
        var options = Options.Create(new AutomationOptions
        {
            MaxRetryCount = 1,
            RetryDelaySeconds = 1,
        });

        var service = new AutomationExecutionService(
            scopeFactory,
            options,
            statusStore,
            NullLogger<AutomationExecutionService>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.RunReportAsync("Scheduler", CancellationToken.None));

        var status = statusStore.GetAll().Single(item => item.Name == "PeriodicReport");
        Assert.False(status.LastSucceeded);
        Assert.Equal(2, status.LastAttemptCount);
        Assert.Equal(1, status.ConsecutiveFailures);
        Assert.Contains("report failed", status.LastError, StringComparison.OrdinalIgnoreCase);
    }

    private static ServiceProvider BuildProvider(RetryCounter counter)
    {
        var services = new ServiceCollection();
        services.AddSingleton(counter);
        services.AddScoped<IAutomationWorkflowService, FakeWorkflowService>();
        return services.BuildServiceProvider();
    }

    private sealed class RetryCounter
    {
        public int FailReminderAttempts { get; set; }

        public int ReminderRuns { get; set; }

        public bool AlwaysFailReport { get; set; }
    }

    private sealed class FakeWorkflowService(RetryCounter counter) : IAutomationWorkflowService
    {
        public Task RunReminderOnceAsync(CancellationToken cancellationToken)
        {
            counter.ReminderRuns++;
            if (counter.ReminderRuns <= counter.FailReminderAttempts)
            {
                throw new InvalidOperationException("reminder failed");
            }

            return Task.CompletedTask;
        }

        public Task RunPeriodicReportOnceAsync(CancellationToken cancellationToken)
        {
            if (counter.AlwaysFailReport)
            {
                throw new InvalidOperationException("report failed");
            }

            return Task.CompletedTask;
        }
    }
}
