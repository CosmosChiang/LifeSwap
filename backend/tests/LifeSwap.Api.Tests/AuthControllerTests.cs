using LifeSwap.Api.Contracts;
using LifeSwap.Api.Controllers;
using LifeSwap.Api.Data;
using LifeSwap.Api.Domain;
using LifeSwap.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LifeSwap.Api.Tests;

public sealed class AuthControllerTests
{
    [Fact]
    public async Task LoginAsync_WhenUserNotFound_ReturnsProblemDetailsUnauthorized()
    {
        await using var dbContext = await CreateDbContextAsync();
        var controller = new AuthController(
            dbContext,
            new FakePasswordHashService(),
            new FakeJwtTokenService());

        var response = await controller.LoginAsync(
            new LoginRequestDto("missing-user", "password"),
            CancellationToken.None);

        var unauthorized = Assert.IsType<ObjectResult>(response.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, unauthorized.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(unauthorized.Value);
        Assert.Equal("Authentication failed.", problem.Title);
        Assert.Equal("Invalid username or password.", problem.Detail);
    }

    [Fact]
    public async Task ChangePasswordAsync_WhenPayloadInvalid_ReturnsProblemDetailsBadRequest()
    {
        await using var dbContext = await CreateDbContextAsync();
        var controller = new AuthController(
            dbContext,
            new FakePasswordHashService(),
            new FakeJwtTokenService());

        var response = await controller.ChangePasswordAsync(
            new ChangePasswordRequestDto(string.Empty, string.Empty),
            CancellationToken.None);

        var badRequest = Assert.IsType<ObjectResult>(response);
        Assert.Equal(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Invalid password update request.", problem.Title);
        Assert.Equal("CurrentPassword and NewPassword are required.", problem.Detail);
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
