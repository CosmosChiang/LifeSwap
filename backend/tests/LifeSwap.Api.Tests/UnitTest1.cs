using LifeSwap.Api.Controllers;
using LifeSwap.Api.Contracts;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Tests;

public sealed class ReportsControllerTests
{
    [Fact]
    public async Task GetSummaryAsync_ReturnsExpectedAggregates()
    {
        await using var dbContext = await CreateDbContextAsync();
        dbContext.TimeOffRequests.AddRange(
            new TimeOffRequest
            {
                EmployeeId = "E001",
                DepartmentCode = "ENG",
                RequestType = RequestType.Overtime,
                RequestDate = new DateOnly(2026, 2, 1),
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(20, 0),
                Status = RequestStatus.Approved,
                Reason = "Release support",
            },
            new TimeOffRequest
            {
                EmployeeId = "E002",
                DepartmentCode = "HR",
                RequestType = RequestType.CompOff,
                RequestDate = new DateOnly(2026, 2, 2),
                Status = RequestStatus.Submitted,
                Reason = "Comp-off day",
            },
            new TimeOffRequest
            {
                EmployeeId = "E003",
                DepartmentCode = "ENG",
                RequestType = RequestType.Overtime,
                RequestDate = new DateOnly(2026, 2, 3),
                StartTime = new TimeOnly(19, 0),
                EndTime = new TimeOnly(21, 0),
                Status = RequestStatus.Rejected,
                Reason = "Urgent task",
            });
        await dbContext.SaveChangesAsync();

        var controller = new ReportsController(dbContext);

        var response = await controller.GetSummaryAsync(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28),
            null,
            null,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<ReportSummaryDto>(okResult.Value);

        Assert.Equal(3, payload.TotalRequests);
        Assert.Equal(1, payload.SubmittedCount);
        Assert.Equal(1, payload.ApprovedCount);
        Assert.Equal(1, payload.RejectedCount);
        Assert.Equal(0, payload.CancelledCount);
        Assert.Equal(2, payload.ApprovedOvertimeHours);
        Assert.Equal(0.3333, payload.ApprovalRate, 4);
    }

    [Fact]
    public async Task GetSummaryAsync_FiltersByDepartmentCode()
    {
        await using var dbContext = await CreateDbContextAsync();
        dbContext.TimeOffRequests.AddRange(
            new TimeOffRequest
            {
                EmployeeId = "E101",
                DepartmentCode = "ENG",
                RequestType = RequestType.Overtime,
                RequestDate = new DateOnly(2026, 2, 10),
                StartTime = new TimeOnly(19, 0),
                EndTime = new TimeOnly(21, 0),
                Status = RequestStatus.Approved,
                Reason = "Engineering shift",
            },
            new TimeOffRequest
            {
                EmployeeId = "E201",
                DepartmentCode = "HR",
                RequestType = RequestType.Overtime,
                RequestDate = new DateOnly(2026, 2, 10),
                StartTime = new TimeOnly(19, 0),
                EndTime = new TimeOnly(21, 0),
                Status = RequestStatus.Approved,
                Reason = "HR shift",
            });
        await dbContext.SaveChangesAsync();

        var controller = new ReportsController(dbContext);

        var response = await controller.GetSummaryAsync(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28),
            null,
            "ENG",
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<ReportSummaryDto>(okResult.Value);

        Assert.Equal(1, payload.TotalRequests);
        Assert.Equal(1, payload.ApprovedCount);
        Assert.Equal(2, payload.ApprovedOvertimeHours);
    }

    [Fact]
    public async Task GetComplianceWarningsAsync_ReturnsWarningAndCriticalByThreshold()
    {
        await using var dbContext = await CreateDbContextAsync();
        dbContext.TimeOffRequests.AddRange(
            new TimeOffRequest
            {
                EmployeeId = "E010",
                DepartmentCode = "ENG",
                RequestType = RequestType.Overtime,
                RequestDate = new DateOnly(2026, 2, 1),
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(22, 0),
                Status = RequestStatus.Approved,
                Reason = "Month start",
            },
            new TimeOffRequest
            {
                EmployeeId = "E010",
                DepartmentCode = "ENG",
                RequestType = RequestType.Overtime,
                RequestDate = new DateOnly(2026, 2, 2),
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(23, 0),
                Status = RequestStatus.Approved,
                Reason = "Month peak",
            },
            new TimeOffRequest
            {
                EmployeeId = "E011",
                DepartmentCode = "OPS",
                RequestType = RequestType.Overtime,
                RequestDate = new DateOnly(2026, 2, 3),
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(21, 0),
                Status = RequestStatus.Approved,
                Reason = "Expected warning",
            },
            new TimeOffRequest
            {
                EmployeeId = "E011",
                DepartmentCode = "OPS",
                RequestType = RequestType.Overtime,
                RequestDate = new DateOnly(2026, 2, 4),
                StartTime = new TimeOnly(18, 0),
                EndTime = new TimeOnly(22, 0),
                Status = RequestStatus.Approved,
                Reason = "Expected warning follow-up",
            });
        await dbContext.SaveChangesAsync();

        var controller = new ReportsController(dbContext);

        var response = await controller.GetComplianceWarningsAsync(
            new DateOnly(2026, 2, 1),
            new DateOnly(2026, 2, 28),
            8,
            null,
            CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(response.Result);
        var payload = Assert.IsType<List<ComplianceWarningDto>>(okResult.Value);

        Assert.Equal(2, payload.Count);
        Assert.Equal("E010", payload[0].EmployeeId);
        Assert.Equal("Critical", payload[0].Severity);
        Assert.Equal("E011", payload[1].EmployeeId);
        Assert.Equal("Warning", payload[1].Severity);
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
}
