using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LifeSwap.Api.Contracts;
using LifeSwap.Api.Controllers;
using LifeSwap.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LifeSwap.Api.Tests;

public class UserControllerTests
{
    [Fact]
    public async Task GetUsers_ReturnsOkResult_WithUserList()
    {
        // Arrange
        var mockService = new Mock<IUserManagementService>();
        mockService.Setup(s => s.GetAllUsersAsync()).ReturnsAsync(new List<UserDto> {
            new UserDto { Id = Guid.NewGuid(), Username = "test", Email = "test@a.com", EmployeeId = "E1", DepartmentCode = "IT", IsActive = true }
        });
        var controller = new UserController(null!, mockService.Object);

        // Act
        var result = await controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var users = Assert.IsAssignableFrom<IEnumerable<UserDto>>(okResult.Value);
        Assert.Single(users);
    }

    [Fact]
    public async Task GetUser_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var mockService = new Mock<IUserManagementService>();
        mockService.Setup(s => s.GetUserByIdAsync(It.IsAny<Guid>())).ReturnsAsync((UserDto?)null);
        var controller = new UserController(null!, mockService.Object);
        var result = await controller.GetUser(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateUser_ReturnsCreatedAtAction()
    {
        var mockService = new Mock<IUserManagementService>();
        var user = new UserDto { Id = Guid.NewGuid(), Username = "new", Email = "n@a.com", EmployeeId = "E2", DepartmentCode = "HR", IsActive = true };
        mockService.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>())).ReturnsAsync(user);
        var controller = new UserController(null!, mockService.Object);
        var result = await controller.CreateUser(new CreateUserDto());
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(user, created.Value);
    }
}
