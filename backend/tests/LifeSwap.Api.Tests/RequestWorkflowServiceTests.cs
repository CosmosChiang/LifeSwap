using LifeSwap.Api.Domain;
using LifeSwap.Api.Services;

namespace LifeSwap.Api.Tests;

public sealed class RequestWorkflowServiceTests
{
    [Fact]
    public void CanSubmit_ReturnsTrue_ForDraftAndReturned()
    {
        var service = new RequestWorkflowService();

        var draft = new TimeOffRequest { Status = RequestStatus.Draft };
        var returned = new TimeOffRequest { Status = RequestStatus.Returned };

        Assert.True(service.CanSubmit(draft));
        Assert.True(service.CanSubmit(returned));
    }

    [Fact]
    public void CanReturn_ReturnsTrue_OnlyForSubmitted()
    {
        var service = new RequestWorkflowService();

        var submitted = new TimeOffRequest { Status = RequestStatus.Submitted };
        var approved = new TimeOffRequest { Status = RequestStatus.Approved };

        Assert.True(service.CanReturn(submitted));
        Assert.False(service.CanReturn(approved));
    }

    [Fact]
    public void CanCancel_ReturnsTrue_ForReturned()
    {
        var service = new RequestWorkflowService();
        var returned = new TimeOffRequest { Status = RequestStatus.Returned };

        Assert.True(service.CanCancel(returned));
    }
}
