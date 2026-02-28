namespace LifeSwap.Api.Services;

public sealed class AutomationOptions
{
    public const string SectionName = "Automation";

    public bool Enabled { get; set; } = true;

    public int ReminderIntervalMinutes { get; set; } = 30;

    public int ReportIntervalMinutes { get; set; } = 1440;

    public int PendingReminderAfterHours { get; set; } = 8;

    public int MaxRetryCount { get; set; } = 2;

    public int RetryDelaySeconds { get; set; } = 3;

    public string ReminderRecipientEmployeeId { get; set; } = "MANAGER";

    public string ReportRecipientEmployeeId { get; set; } = "ADMIN001";
}
