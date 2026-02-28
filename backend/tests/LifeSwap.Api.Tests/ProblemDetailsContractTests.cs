using System.Security.Claims;
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
using Microsoft.Extensions.Options;

namespace LifeSwap.Api.Tests;

public sealed class ProblemDetailsContractTests
{
    private const string Rfc7807Type = "https://datatracker.ietf.org/doc/html/rfc7807";

    [Fact]
    public async Task RequestsController_SubmitInvalidTransition_ReturnsRfc7807ProblemDetails()
    {
        await using var dbContext = await CreateDbContextAsync();

        var request = new TimeOffRequest
        {
            EmployeeId = "E001",
            ApplicantName = "Alice",
            DepartmentCode = "ENG",
            RequestType = RequestType.Overtime,
            RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            Status = RequestStatus.Approved,
            OvertimeProject = "P1",
            OvertimeContent = "Work",
            OvertimeReason = "Reason",
            Reason = "Reason",
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

        var response = await controller.SubmitAsync(request.Id, CancellationToken.None);

        var badRequest = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid request status transition.", problem.Title);
        Assert.Equal("Only draft or returned requests can be submitted.", problem.Detail);
        Assert.Equal(Rfc7807Type, problem.Type);
    }

    [Fact]
    public async Task AuthController_LoginUserNotFound_ReturnsRfc7807ProblemDetails()
    {
        await using var dbContext = await CreateDbContextAsync();

        var controller = new AuthController(
            dbContext,
            new FakePasswordHashService(),
            new FakeJwtTokenService());

        var response = await controller.LoginAsync(
            new LoginRequestDto("missing", "password"),
            CancellationToken.None);

        var unauthorized = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorized.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(unauthorized.Value);
        Assert.Equal("Authentication failed.", problem.Title);
        Assert.Equal("Invalid username or password.", problem.Detail);
        Assert.Equal(Rfc7807Type, problem.Type);
    }

    [Fact]
    public async Task ReportsController_InvalidMonthlyLimit_ReturnsRfc7807ProblemDetails()
    {
        await using var dbContext = await CreateDbContextAsync();
        var controller = new ReportsController(dbContext);

        var response = await controller.GetComplianceWarningsAsync(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28),
            null,
            0,
            null,
            null,
            CancellationToken.None);

        var badRequest = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid monthly overtime limit.", problem.Title);
        Assert.Equal("monthlyOvertimeHourLimit must be greater than zero.", problem.Detail);
        Assert.Equal(Rfc7807Type, problem.Type);
    }

    [Fact]
    public async Task AutomationController_RunReportFailure_ReturnsRfc7807ProblemDetails()
    {
        var controller = new AutomationController(
            new ThrowingExecutionService(),
            new AutomationSchedulerStateService(Options.Create(new AutomationOptions
            {
                Enabled = true,
            })),
            new AutomationExecutionStatusStore(),
            Options.Create(new AutomationOptions()),
            NullLogger<AutomationController>.Instance);

        var response = await controller.RunReportAsync(CancellationToken.None);

        var error = Assert.IsType<ObjectResult>(response);
        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(error.Value);
        Assert.Equal("Automation workflow failed.", problem.Title);
        Assert.Equal("Report workflow failed.", problem.Detail);
        Assert.Equal(Rfc7807Type, problem.Type);
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

    private sealed class ThrowingExecutionService : IAutomationExecutionService
    {
        public Task RunReminderAsync(string trigger, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task RunReportAsync(string trigger, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("report failed");
        }
    }

    private sealed class FakePasswordHashService : IPasswordHashService
    {
        public string HashPassword(string password)
        {
            return password;
        }

        public bool VerifyPassword(string password, string hash)
        {
            return string.Equals(password, hash, StringComparison.Ordinal);
        }
    }

    private sealed class FakeJwtTokenService : IJwtTokenService
    {
        public string GenerateToken(User user, IEnumerable<Role> roles)
        {
            return "fake-token";
        }
    }
}
