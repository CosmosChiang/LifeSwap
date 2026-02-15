using LifeSwap.Api.Domain;

namespace LifeSwap.Api.Services;

public interface ITeamsNotificationService
{
    /// <summary>
    /// Sends request status change notification to Microsoft Teams.
    /// </summary>
    Task SendRequestStatusChangedAsync(TimeOffRequest request, string actionName, CancellationToken cancellationToken);

    /// <summary>
    /// Sends plain text notification to Microsoft Teams.
    /// </summary>
    Task SendMessageAsync(string message, CancellationToken cancellationToken);
}
