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

public class RoleControllerTests
{
    [Fact]
    public async Task GetRoles_ReturnsOkResult_WithRoleList()
    {
        var mockService = new Mock<IRoleManagementService>();
        mockService.Setup(s => s.GetAllRolesAsync()).ReturnsAsync(new List<RoleDto> {
            new RoleDto { Id = Guid.NewGuid(), Name = "Admin", Description = "desc" }
        });
        var controller = new RoleController(mockService.Object);
        var result = await controller.GetRoles();
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var roles = Assert.IsAssignableFrom<IEnumerable<RoleDto>>(okResult.Value);
        Assert.Single(roles);
    }

    [Fact]
    public async Task GetRole_ReturnsNotFound_WhenRoleDoesNotExist()
    {
        var mockService = new Mock<IRoleManagementService>();
        mockService.Setup(s => s.GetRoleByIdAsync(It.IsAny<Guid>())).ReturnsAsync((RoleDto?)null);
        var controller = new RoleController(mockService.Object);
        var result = await controller.GetRole(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task CreateRole_ReturnsCreatedAtAction()
    {
        var mockService = new Mock<IRoleManagementService>();
        var role = new RoleDto { Id = Guid.NewGuid(), Name = "Manager", Description = "desc" };
        mockService.Setup(s => s.CreateRoleAsync(It.IsAny<CreateRoleDto>())).ReturnsAsync(role);
        var controller = new RoleController(mockService.Object);
        var result = await controller.CreateRole(new CreateRoleDto());
        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(role, created.Value);
    }
}
