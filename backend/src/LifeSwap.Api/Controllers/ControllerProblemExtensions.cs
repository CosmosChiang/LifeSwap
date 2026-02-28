using Microsoft.AspNetCore.Mvc;

namespace LifeSwap.Api.Controllers;

public static class ControllerProblemExtensions
{
    private const string Rfc7807Type = "https://datatracker.ietf.org/doc/html/rfc7807";

    /// <summary>
    /// Creates a standardized RFC7807 problem response with the provided status.
    /// </summary>
    public static ObjectResult CreateProblemResponse(
        this ControllerBase controller,
        int statusCode,
        string title,
        string detail)
    {
        return controller.Problem(
            title: title,
            detail: detail,
            statusCode: statusCode,
            type: Rfc7807Type);
    }

    /// <summary>
    /// Creates a standardized RFC7807 validation problem response.
    /// </summary>
    public static ObjectResult CreateValidationProblemResponse(
        this ControllerBase controller,
        string title,
        string detail)
    {
        return controller.CreateProblemResponse(
            StatusCodes.Status400BadRequest,
            title,
            detail);
    }

    /// <summary>
    /// Creates a standardized RFC7807 authentication problem response.
    /// </summary>
    public static ObjectResult CreateAuthenticationProblemResponse(
        this ControllerBase controller,
        string detail)
    {
        return controller.CreateProblemResponse(
            StatusCodes.Status401Unauthorized,
            "Authentication failed.",
            detail);
    }

    /// <summary>
    /// Creates a standardized RFC7807 internal error problem response.
    /// </summary>
    public static ObjectResult CreateInternalProblemResponse(
        this ControllerBase controller,
        string title,
        string detail)
    {
        return controller.CreateProblemResponse(
            StatusCodes.Status500InternalServerError,
            title,
            detail);
    }
}
