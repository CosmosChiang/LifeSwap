using System.Text;
using System.Text.Json;
using LifeSwap.Api.Domain;
using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Services;

public sealed class TeamsNotificationService(
    HttpClient httpClient,
    IOptions<TeamsNotificationOptions> options,
    ILogger<TeamsNotificationService> logger) : ITeamsNotificationService
{
    /// <summary>
    /// Sends request status change notification to Microsoft Teams.
    /// </summary>
    public async Task SendRequestStatusChangedAsync(TimeOffRequest request, string actionName, CancellationToken cancellationToken)
    {
        var message = $"[LifeSwap] Request {request.Id} is {request.Status} ({actionName}). Employee: {request.EmployeeId}, Department: {request.DepartmentCode}, Date: {request.RequestDate:yyyy-MM-dd}.";
        await SendMessageAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends plain text notification to Microsoft Teams.
    /// </summary>
    public async Task SendMessageAsync(string message, CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled || string.IsNullOrWhiteSpace(options.Value.WebhookUrl))
        {
            return;
        }

        var payload = new
        {
            text = message,
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await httpClient.PostAsync(options.Value.WebhookUrl, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Teams notification failed with status code {StatusCode}.", response.StatusCode);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Teams notification threw an exception.");
        }
    }
}
