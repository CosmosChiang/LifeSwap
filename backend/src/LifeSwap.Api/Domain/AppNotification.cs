namespace LifeSwap.Api.Domain;

public sealed class AppNotification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string RecipientEmployeeId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}