using System.Security.Claims;
using LifeSwap.Api.Contracts;
using LifeSwap.Api.Controllers;
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

public sealed class ControllersCoverageTests
{
    private const string Rfc7807Type = "https://datatracker.ietf.org/doc/html/rfc7807";

    [Fact]
    public async Task NotificationsController_GetMyNotificationsAsync_ReturnsCurrentEmployeeOnly()
    {
        await using var dbContext = await CreateDbContextAsync();
        dbContext.Notifications.AddRange(
            new AppNotification
            {
                RecipientEmployeeId = "E001",
                Title = "A",
                Message = "M1",
                IsRead = false,
            },
            new AppNotification
            {
                RecipientEmployeeId = "E002",
                Title = "B",
                Message = "M2",
                IsRead = false,
            });
        await dbContext.SaveChangesAsync();

        var controller = new NotificationsController(dbContext)
        {
            ControllerContext = BuildControllerContext("E001"),
        };

        var response = await controller.GetMyNotificationsAsync(false, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsAssignableFrom<IReadOnlyCollection<NotificationItemDto>>(ok.Value);

        Assert.Single(payload);
        Assert.All(payload, item => Assert.Equal("E001", item.RecipientEmployeeId));
    }

    [Fact]
    public async Task NotificationsController_MarkAsReadAsync_UpdatesIsReadFlag()
    {
        await using var dbContext = await CreateDbContextAsync();
        var notification = new AppNotification
        {
            RecipientEmployeeId = "E001",
            Title = "A",
            Message = "M1",
            IsRead = false,
        };

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();

        var controller = new NotificationsController(dbContext)
        {
            ControllerContext = BuildControllerContext("E001"),
        };

        var actionResult = await controller.MarkAsReadAsync(notification.Id, CancellationToken.None);
        Assert.IsType<OkResult>(actionResult);

        var updated = await dbContext.Notifications.FirstAsync(item => item.Id == notification.Id);
        Assert.True(updated.IsRead);
    }

    [Fact]
    public async Task ReportsController_GetSummaryAsync_FiltersByDepartmentCodePrefix()
    {
        await using var dbContext = await CreateDbContextAsync();
        dbContext.TimeOffRequests.AddRange(
            new TimeOffRequest
            {
                EmployeeId = "E001",
                DepartmentCode = "ENG",
                RequestType = RequestType.Overtime,
                RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                Status = RequestStatus.Approved,
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(20, 0),
                Reason = "A",
            },
            new TimeOffRequest
            {
                EmployeeId = "E002",
                DepartmentCode = "OPS",
                RequestType = RequestType.Overtime,
                RequestDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
                Status = RequestStatus.Approved,
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(19, 0),
                Reason = "B",
            });
        await dbContext.SaveChangesAsync();

        var controller = new ReportsController(dbContext);
        var response = await controller.GetSummaryAsync(
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            RequestType.Overtime,
            null,
            "EN",
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        var summary = Assert.IsType<ReportSummaryDto>(ok.Value);

        Assert.Equal(1, summary.TotalRequests);
        Assert.Equal("EN", summary.DepartmentCode);
    }

    [Fact]
    public async Task ReportsController_GetComplianceWarningsAsync_InvalidLimit_ReturnsProblemDetails()
    {
        await using var dbContext = await CreateDbContextAsync();
        var controller = new ReportsController(dbContext);

        var response = await controller.GetComplianceWarningsAsync(
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(1)),
            null,
            0,
            null,
            null,
            CancellationToken.None);

        var badRequest = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid monthly overtime limit.", problem.Title);
        Assert.Equal(Rfc7807Type, problem.Type);
    }

    [Fact]
    public async Task AutomationController_RunReportAsync_WhenFailed_ReturnsProblemDetails()
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

        var result = await controller.RunReportAsync(CancellationToken.None);
        var error = Assert.IsType<ObjectResult>(result);

        Assert.Equal(StatusCodes.Status500InternalServerError, error.StatusCode);
        var problem = Assert.IsType<ProblemDetails>(error.Value);
        Assert.Equal("Automation workflow failed.", problem.Title);
        Assert.Equal(Rfc7807Type, problem.Type);
    }

    private static ControllerContext BuildControllerContext(string employeeId)
    {
        var identity = new ClaimsIdentity(
        [
            new Claim("EmployeeId", employeeId),
        ],
        "TestAuth");

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
}
