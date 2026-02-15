using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Services;

public sealed class AutomationWorkflowService(
    AppDbContext dbContext,
    ITeamsNotificationService teamsNotificationService,
    IOptions<AutomationOptions> options,
    ILogger<AutomationWorkflowService> logger) : IAutomationWorkflowService
{
    /// <summary>
    /// Executes pending-review reminder workflow once.
    /// </summary>
    public async Task RunReminderOnceAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var reminderThreshold = now.AddHours(-options.Value.PendingReminderAfterHours);
        var todayStart = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, TimeSpan.Zero);

        var pendingCandidates = await dbContext.TimeOffRequests
            .Where(request =>
                request.Status == RequestStatus.Submitted &&
                request.SubmittedAt.HasValue)
            .ToListAsync(cancellationToken);

        var pendingRequests = pendingCandidates
            .Where(request => request.SubmittedAt is not null && request.SubmittedAt.Value <= reminderThreshold)
            .ToList();

        var existingReminders = await dbContext.Notifications
            .Where(notification => notification.Title == "待審提醒")
            .ToListAsync(cancellationToken);

        var todayReminders = existingReminders
            .Where(notification => notification.CreatedAt >= todayStart)
            .ToList();

        foreach (var request in pendingRequests)
        {
            var alreadyNotifiedToday = todayReminders
                .Any(notification => notification.Message.Contains(request.Id.ToString(), StringComparison.Ordinal));

            if (alreadyNotifiedToday)
            {
                continue;
            }

            var reminderMessage = $"申請 {request.Id} 仍在待審（員工 {request.EmployeeId}，部門 {request.DepartmentCode}，送審時間 {request.SubmittedAt:yyyy-MM-dd HH:mm} UTC）。";
            dbContext.Notifications.Add(new AppNotification
            {
                RecipientEmployeeId = options.Value.ReminderRecipientEmployeeId,
                Title = "待審提醒",
                Message = reminderMessage,
            });

            await teamsNotificationService.SendMessageAsync($"[LifeSwap][Reminder] {reminderMessage}", cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Automation reminder workflow completed at {Time}.", now);
    }

    /// <summary>
    /// Executes periodic report workflow once.
    /// </summary>
    public async Task RunPeriodicReportOnceAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var startDate = DateOnly.FromDateTime(now.UtcDateTime.Date.AddDays(-1));
        var endDate = DateOnly.FromDateTime(now.UtcDateTime.Date);

        var requests = await dbContext.TimeOffRequests
            .AsNoTracking()
            .Where(request => request.RequestDate >= startDate && request.RequestDate <= endDate)
            .ToListAsync(cancellationToken);

        var approvedOvertimeHours = Math.Round(
            requests
                .Where(request => request.Status == RequestStatus.Approved && request.RequestType == RequestType.Overtime)
                .Sum(CalculateHours),
            2);

        var reportMessage =
            $"日報（{startDate:yyyy-MM-dd} ~ {endDate:yyyy-MM-dd}） | 總申請: {requests.Count}, 送審中: {requests.Count(request => request.Status == RequestStatus.Submitted)}, 核准: {requests.Count(request => request.Status == RequestStatus.Approved)}, 拒絕: {requests.Count(request => request.Status == RequestStatus.Rejected)}, 取消: {requests.Count(request => request.Status == RequestStatus.Cancelled)}, 核准加班時數: {approvedOvertimeHours}.";

        dbContext.Notifications.Add(new AppNotification
        {
            RecipientEmployeeId = options.Value.ReportRecipientEmployeeId,
            Title = "定期報告",
            Message = reportMessage,
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await teamsNotificationService.SendMessageAsync($"[LifeSwap][Report] {reportMessage}", cancellationToken);

        logger.LogInformation("Automation periodic report workflow completed at {Time}.", now);
    }

    /// <summary>
    /// Calculates request duration in hours.
    /// </summary>
    private static double CalculateHours(TimeOffRequest request)
    {
        if (request.StartTime is null || request.EndTime is null)
        {
            return 0;
        }

        var duration = request.EndTime.Value.ToTimeSpan() - request.StartTime.Value.ToTimeSpan();
        if (duration <= TimeSpan.Zero)
        {
            return 0;
        }

        return duration.TotalHours;
    }
}
