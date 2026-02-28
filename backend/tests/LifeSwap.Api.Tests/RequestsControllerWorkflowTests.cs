using LifeSwap.Api.Controllers;
using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using LifeSwap.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Security.Claims;

namespace LifeSwap.Api.Tests;

public sealed class RequestsControllerWorkflowTests
{
    [Fact]
    public async Task SubmitAsync_FromReturned_ClearsReviewFieldsAndSubmits()
    {
        await using var dbContext = await CreateDbContextAsync();

        var request = new TimeOffRequest
        {
            EmployeeId = "E001",
            ApplicantName = "Alice",
            DepartmentCode = "ENG",
            RequestType = RequestType.Overtime,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            OvertimeStartAt = DateTimeOffset.UtcNow.AddHours(-3),
            OvertimeEndAt = DateTimeOffset.UtcNow.AddHours(-1),
            OvertimeProject = "P1",
            OvertimeContent = "Work",
            OvertimeReason = "Deadline",
            Reason = "Deadline",
            Status = RequestStatus.Returned,
            ReviewerId = "M001",
            ReviewComment = "Please fix details",
            ReviewedAt = DateTimeOffset.UtcNow.AddHours(-1),
        };

        dbContext.TimeOffRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new NoOpTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContext("E001"),
        };

        var result = await controller.SubmitAsync(request.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<TimeOffRequest>(ok.Value);

        Assert.Equal(RequestStatus.Submitted, payload.Status);
        Assert.NotNull(payload.SubmittedAt);
        Assert.Null(payload.ReviewerId);
        Assert.Null(payload.ReviewComment);
        Assert.Null(payload.ReviewedAt);

        var notifications = await dbContext.Notifications
            .Where(item => item.RecipientEmployeeId == request.EmployeeId)
            .ToListAsync();
        Assert.Single(notifications);
        Assert.Equal("申請已重新送審", notifications[0].Title);
    }

    [Fact]
    public async Task CancelAsync_FromReturned_ReturnsBadRequest()
    {
        await using var dbContext = await CreateDbContextAsync();

        var request = new TimeOffRequest
        {
            EmployeeId = "E001",
            ApplicantName = "Alice",
            DepartmentCode = "ENG",
            RequestType = RequestType.Overtime,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            OvertimeStartAt = DateTimeOffset.UtcNow.AddHours(-3),
            OvertimeEndAt = DateTimeOffset.UtcNow.AddHours(-1),
            OvertimeProject = "P1",
            OvertimeContent = "Work",
            OvertimeReason = "Deadline",
            Reason = "Deadline",
            Status = RequestStatus.Returned,
        };

        dbContext.TimeOffRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new NoOpTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContext("E001"),
        };

        var result = await controller.CancelAsync(request.Id, CancellationToken.None);

        var badRequest = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Only draft or submitted requests can be cancelled.", problem.Detail);

        var persisted = await dbContext.TimeOffRequests.FirstAsync(entity => entity.Id == request.Id);
        Assert.Equal(RequestStatus.Returned, persisted.Status);
        Assert.Null(persisted.CancelledAt);
    }

    [Fact]
    public async Task CancelAsync_WhenTeamsThrows_StillReturnsOkAndPersistsNotification()
    {
        await using var dbContext = await CreateDbContextAsync();

        var request = new TimeOffRequest
        {
            EmployeeId = "E001",
            ApplicantName = "Alice",
            DepartmentCode = "ENG",
            RequestType = RequestType.Overtime,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            OvertimeStartAt = DateTimeOffset.UtcNow.AddHours(-3),
            OvertimeEndAt = DateTimeOffset.UtcNow.AddHours(-1),
            OvertimeProject = "P1",
            OvertimeContent = "Work",
            OvertimeReason = "Deadline",
            Reason = "Deadline",
            Status = RequestStatus.Submitted,
        };

        dbContext.TimeOffRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new ThrowingTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContext("E001"),
        };

        var result = await controller.CancelAsync(request.Id, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<TimeOffRequest>(ok.Value);
        Assert.Equal(RequestStatus.Cancelled, payload.Status);

        var persisted = await dbContext.TimeOffRequests.FirstAsync(entity => entity.Id == request.Id);
        Assert.Equal(RequestStatus.Cancelled, persisted.Status);

        var notifications = await dbContext.Notifications
            .Where(item => item.RecipientEmployeeId == request.EmployeeId)
            .ToListAsync();
        Assert.Single(notifications);
        Assert.Equal("申請已取消", notifications[0].Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotOwnerAndNotPrivileged_ReturnsForbid()
    {
        await using var dbContext = await CreateDbContextAsync();

        var request = new TimeOffRequest
        {
            EmployeeId = "E001",
            ApplicantName = "Alice",
            DepartmentCode = "ENG",
            RequestType = RequestType.Overtime,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = RequestStatus.Submitted,
            Reason = "Test",
            OvertimeProject = "P1",
            OvertimeContent = "Work",
            OvertimeReason = "Deadline",
        };

        dbContext.TimeOffRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new NoOpTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContext("E999"),
        };

        var result = await controller.GetByIdAsync(request.Id, CancellationToken.None);
        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task CancelAsync_WhenAdministratorNotOwner_AllowsOperation()
    {
        await using var dbContext = await CreateDbContextAsync();

        var request = new TimeOffRequest
        {
            EmployeeId = "E001",
            ApplicantName = "Alice",
            DepartmentCode = "ENG",
            RequestType = RequestType.Overtime,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            OvertimeStartAt = DateTimeOffset.UtcNow.AddHours(-3),
            OvertimeEndAt = DateTimeOffset.UtcNow.AddHours(-1),
            OvertimeProject = "P1",
            OvertimeContent = "Work",
            OvertimeReason = "Deadline",
            Reason = "Deadline",
            Status = RequestStatus.Submitted,
        };

        dbContext.TimeOffRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new NoOpTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContext("ADMIN001", "Administrator"),
        };

        var result = await controller.CancelAsync(request.Id, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<TimeOffRequest>(ok.Value);
        Assert.Equal(RequestStatus.Cancelled, payload.Status);
    }

    [Fact]
    public async Task ApproveAsync_UsesReviewerFromClaims_NotFromPayload()
    {
        await using var dbContext = await CreateDbContextAsync();

        var request = new TimeOffRequest
        {
            EmployeeId = "E001",
            ApplicantName = "Alice",
            DepartmentCode = "ENG",
            RequestType = RequestType.Overtime,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            OvertimeProject = "P1",
            OvertimeContent = "Work",
            OvertimeReason = "Deadline",
            Reason = "Deadline",
            Status = RequestStatus.Submitted,
        };

        dbContext.TimeOffRequests.Add(request);
        await dbContext.SaveChangesAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new NoOpTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContext("M777", "Manager"),
        };

        var result = await controller.ApproveAsync(
            request.Id,
            new ReviewRequestDto("Looks good"),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<TimeOffRequest>(ok.Value);
        Assert.Equal(RequestStatus.Approved, payload.Status);
        Assert.Equal("M777", payload.ReviewerId);
        Assert.Equal("Looks good", payload.ReviewComment);
    }

    [Fact]
    public async Task CreateAsync_WhenNonPrivilegedAndEmployeeIdMismatch_ReturnsForbid()
    {
        await using var dbContext = await CreateDbContextAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new NoOpTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContext("E001"),
        };

        var input = new CreateRequestDto(
            EmployeeId: "E999",
            OvertimeStartAt: DateTimeOffset.UtcNow.AddHours(-3),
            OvertimeEndAt: DateTimeOffset.UtcNow.AddHours(-1),
            OvertimeProject: "Project A",
            OvertimeContent: "Work item",
            OvertimeReason: "Deadline");

        var result = await controller.CreateAsync(input, CancellationToken.None);
        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task CreateAsync_WhenAdministratorWithDifferentEmployeeId_CreatesForTargetEmployee()
    {
        await using var dbContext = await CreateDbContextAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new NoOpTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContext("ADMIN001", "Administrator"),
        };

        var input = new CreateRequestDto(
            EmployeeId: "E888",
            OvertimeStartAt: DateTimeOffset.UtcNow.AddHours(-4),
            OvertimeEndAt: DateTimeOffset.UtcNow.AddHours(-2),
            OvertimeProject: "Project B",
            OvertimeContent: "Support",
            OvertimeReason: "Peak");

        var result = await controller.CreateAsync(input, CancellationToken.None);
        var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
        var payload = Assert.IsType<TimeOffRequest>(created.Value);

        Assert.Equal("E888", payload.EmployeeId);
        Assert.Equal(RequestStatus.Draft, payload.Status);
    }

    [Fact]
    public async Task CreateAsync_WithoutEmployeeClaim_ReturnsForbid()
    {
        await using var dbContext = await CreateDbContextAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new NoOpTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContextWithoutEmployeeId("Employee"),
        };

        var input = new CreateRequestDto(
            EmployeeId: "E001",
            OvertimeStartAt: DateTimeOffset.UtcNow.AddHours(-3),
            OvertimeEndAt: DateTimeOffset.UtcNow.AddHours(-1),
            OvertimeProject: "Project C",
            OvertimeContent: "Fix",
            OvertimeReason: "Urgent");

        var result = await controller.CreateAsync(input, CancellationToken.None);
        Assert.IsType<ForbidResult>(result.Result);
    }

    [Fact]
    public async Task GetAllAsync_WhenNonPrivilegedRequestsOtherEmployee_ReturnsForbid()
    {
        await using var dbContext = await CreateDbContextAsync();
        dbContext.TimeOffRequests.Add(new TimeOffRequest
        {
            EmployeeId = "E001",
            ApplicantName = "Alice",
            DepartmentCode = "ENG",
            RequestType = RequestType.Overtime,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            OvertimeProject = "P1",
            OvertimeContent = "Work",
            OvertimeReason = "Deadline",
            Reason = "Deadline",
        });
        await dbContext.SaveChangesAsync();

        var controller = new RequestsController(
            dbContext,
            new RequestWorkflowService(),
            new NoOpTeamsNotificationService(),
            NullLogger<RequestsController>.Instance)
        {
            ControllerContext = BuildControllerContext("E002"),
        };

        var result = await controller.GetAllAsync("E001", null, CancellationToken.None);
        Assert.IsType<ForbidResult>(result.Result);
    }

    private static ControllerContext BuildControllerContext(string employeeId, params string[] roles)
    {
        var claims = new List<Claim>
        {
            new("EmployeeId", employeeId),
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var identity = new ClaimsIdentity(claims, "TestAuth");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity),
            },
        };
    }

    private static ControllerContext BuildControllerContextWithoutEmployeeId(params string[] roles)
    {
        var claims = roles.Select(role => new Claim(ClaimTypes.Role, role));
        var identity = new ClaimsIdentity(claims, "TestAuth");

        return new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity),
            },
        };
    }

    private static async Task<AppDbContext> CreateDbContextAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();
        return dbContext;
    }

    private sealed class NoOpTeamsNotificationService : ITeamsNotificationService
    {
        public Task SendRequestStatusChangedAsync(TimeOffRequest request, string actionName, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task SendMessageAsync(string message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingTeamsNotificationService : ITeamsNotificationService
    {
        public Task SendRequestStatusChangedAsync(TimeOffRequest request, string actionName, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Teams unavailable");
        }

        public Task SendMessageAsync(string message, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Teams unavailable");
        }
    }
}
