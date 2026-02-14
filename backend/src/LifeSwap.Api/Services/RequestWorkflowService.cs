using LifeSwap.Api.Domain;

namespace LifeSwap.Api.Services;

public sealed class RequestWorkflowService : IRequestWorkflowService
{
    /// <summary>
    /// Determines whether a request can be submitted for review.
    /// </summary>
    public bool CanSubmit(TimeOffRequest request)
    {
        return request.Status is RequestStatus.Draft;
    }

    /// <summary>
    /// Determines whether a request can be approved by a reviewer.
    /// </summary>
    public bool CanApprove(TimeOffRequest request)
    {
        return request.Status is RequestStatus.Submitted;
    }

    /// <summary>
    /// Determines whether a request can be rejected by a reviewer.
    /// </summary>
    public bool CanReject(TimeOffRequest request)
    {
        return request.Status is RequestStatus.Submitted;
    }

    /// <summary>
    /// Determines whether a request can be cancelled by the applicant.
    /// </summary>
    public bool CanCancel(TimeOffRequest request)
    {
        return request.Status is RequestStatus.Draft or RequestStatus.Submitted;
    }
}