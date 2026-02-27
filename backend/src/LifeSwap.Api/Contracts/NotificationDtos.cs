namespace LifeSwap.Api.Contracts;

public sealed class NotificationItemDto
{
    public Guid Id { get; init; }

    public string RecipientEmployeeId { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    public bool IsRead { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}
