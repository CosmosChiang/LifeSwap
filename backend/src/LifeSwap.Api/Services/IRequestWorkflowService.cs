using LifeSwap.Api.Domain;

namespace LifeSwap.Api.Services;

public interface IRequestWorkflowService
{
    bool CanSubmit(TimeOffRequest request);

    bool CanApprove(TimeOffRequest request);

    bool CanReject(TimeOffRequest request);

    bool CanCancel(TimeOffRequest request);
}