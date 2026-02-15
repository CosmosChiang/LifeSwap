using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using LifeSwap.Api.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Tests;

public sealed class AutomationWorkflowServiceTests
{
    [Fact]
    public async Task RunReminderOnceAsync_CreatesPendingReviewReminder()
    {
        await using var dbContext = await CreateDbContextAsync();
        dbContext.TimeOffRequests.Add(new TimeOffRequest
        {
            EmployeeId = "E900",
            DepartmentCode = "ENG",
            RequestType = RequestType.Overtime,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            StartTime = new TimeOnly(18, 0),
            EndTime = new TimeOnly(20, 0),
            Status = RequestStatus.Submitted,
            SubmittedAt = DateTimeOffset.UtcNow.AddHours(-10),
            Reason = "Reminder target",
        });
        await dbContext.SaveChangesAsync();

        var teams = new FakeTeamsNotificationService();
        var options = Options.Create(new AutomationOptions
        {
            PendingReminderAfterHours = 8,
            ReminderRecipientEmployeeId = "MANAGER",
        });

        var service = new AutomationWorkflowService(dbContext, teams, options, NullLogger<AutomationWorkflowService>.Instance);
        await service.RunReminderOnceAsync(CancellationToken.None);

        var reminders = await dbContext.Notifications
            .Where(notification => notification.Title == "待審提醒")
            .ToListAsync();

        Assert.Single(reminders);
        Assert.Equal("MANAGER", reminders[0].RecipientEmployeeId);
        Assert.Single(teams.Messages);
        Assert.Contains("Reminder", teams.Messages[0]);
    }

    [Fact]
    public async Task RunPeriodicReportOnceAsync_CreatesPeriodicReportNotification()
    {
        await using var dbContext = await CreateDbContextAsync();
        dbContext.TimeOffRequests.AddRange(
            new TimeOffRequest
            {
                EmployeeId = "E901",
                DepartmentCode = "OPS",
                RequestType = RequestType.Overtime,
                RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(22, 0),
                Status = RequestStatus.Approved,
                Reason = "Report target",
            },
            new TimeOffRequest
            {
                EmployeeId = "E902",
                DepartmentCode = "OPS",
                RequestType = RequestType.CompOff,
                RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                Status = RequestStatus.Submitted,
                Reason = "Report target 2",
            });
        await dbContext.SaveChangesAsync();

        var teams = new FakeTeamsNotificationService();
        var options = Options.Create(new AutomationOptions
        {
            ReportRecipientEmployeeId = "HR",
        });

        var service = new AutomationWorkflowService(dbContext, teams, options, NullLogger<AutomationWorkflowService>.Instance);
        await service.RunPeriodicReportOnceAsync(CancellationToken.None);

        var reports = await dbContext.Notifications
            .Where(notification => notification.Title == "定期報告")
            .ToListAsync();

        Assert.Single(reports);
        Assert.Equal("HR", reports[0].RecipientEmployeeId);
        Assert.Contains("總申請", reports[0].Message);
        Assert.Single(teams.Messages);
        Assert.Contains("Report", teams.Messages[0]);
    }

    private static async Task<AppDbContext> CreateDbContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        return dbContext;
    }

    private sealed class FakeTeamsNotificationService : ITeamsNotificationService
    {
        public List<string> Messages { get; } = [];

        public Task SendRequestStatusChangedAsync(TimeOffRequest request, string actionName, CancellationToken cancellationToken)
        {
            Messages.Add($"StatusChanged:{request.Id}:{actionName}");
            return Task.CompletedTask;
        }

        public Task SendMessageAsync(string message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
