namespace LifeSwap.Api.Services;

public sealed class TeamsNotificationOptions
{
    public const string SectionName = "TeamsNotifications";

    public bool Enabled { get; set; }

    public string? WebhookUrl { get; set; }
}
