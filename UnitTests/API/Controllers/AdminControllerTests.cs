using API.Controllers;
using Application.User.DTOs;
using Domain.Entities;
using Domain.Enumerators;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MockQueryable;
using UnitTests.Helpers;

namespace UnitTests.API.Controllers;

public class AdminControllerTests
{
    private readonly Mock<UserManager<User>> _userManager;
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _userManager = CreateUserManagerMock();
        // ControllerTestFactory wires the HttpContext; AdminController needs no Mediator
        // but Create still provides a valid ControllerContext with ServiceProvider.
        (_, _controller) = ControllerTestFactory.Create(new AdminController(_userManager.Object));
    }

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static User BuildUser(string id = "user-1", string email = "user@test.com") => new()
    {
        Id = id,
        UserName = email,
        Email = email,
        EmailConfirmed = true
    };

    // ── GetUsers ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_ReturnsOkWithAllUsersAndRoles()
    {
        var user1 = BuildUser("u1", "alice@test.com");
        var user2 = BuildUser("u2", "bob@test.com");
        var users = new List<User> { user1, user2 };

        _userManager.Setup(m => m.Users).Returns(users.BuildMock());
        _userManager.Setup(m => m.GetRolesAsync(user1)).ReturnsAsync([Roles.Admin]);
        _userManager.Setup(m => m.GetRolesAsync(user2)).ReturnsAsync([Roles.Enrolled]);

        var result = await _controller.GetUsers();

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dtos = ok.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject.ToList();
        dtos.Should().HaveCount(2);
        dtos[0].Id.Should().Be("u1");
        dtos[0].Role.Should().Be(Roles.Admin);
        dtos[1].Id.Should().Be("u2");
        dtos[1].Role.Should().Be(Roles.Enrolled);
    }

    // ── AddRoleToUser ─────────────────────────────────────────────────────────

    [Fact]
    public async Task AddRoleToUser_WhenUserNotFound_ReturnsNotFound()
    {
        _userManager.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((User?)null);

        var result = await _controller.AddRoleToUser("missing", Roles.Admin);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddRoleToUser_WhenRoleInvalid_ReturnsBadRequest()
    {
        var user = BuildUser();
        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);

        var result = await _controller.AddRoleToUser(user.Id, "InvalidRole");

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddRoleToUser_WhenSuccess_ReturnsOk()
    {
        var user = BuildUser();
        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager
            .Setup(m => m.AddToRoleAsync(user, Roles.Admin))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.AddRoleToUser(user.Id, Roles.Admin);

        result.Should().BeOfType<OkResult>();
    }

    [Fact]
    public async Task AddRoleToUser_WhenIdentityFails_ReturnsBadRequest()
    {
        var user = BuildUser();
        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager
            .Setup(m => m.AddToRoleAsync(user, Roles.Admin))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Code = "RoleError", Description = "Role assignment failed" }));

        var result = await _controller.AddRoleToUser(user.Id, Roles.Admin);

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    // ── RemoveUserFromRole ────────────────────────────────────────────────────

    [Fact]
    public async Task RemoveUserFromRole_WhenUserNotFound_ReturnsNotFound()
    {
        _userManager.Setup(m => m.FindByIdAsync("ghost")).ReturnsAsync((User?)null);

        var result = await _controller.RemoveUserFromRole("ghost", Roles.Enrolled);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RemoveUserFromRole_WhenSuccess_ReturnsOk()
    {
        var user = BuildUser();
        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager
            .Setup(m => m.RemoveFromRoleAsync(user, Roles.Enrolled))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.RemoveUserFromRole(user.Id, Roles.Enrolled);

        result.Should().BeOfType<OkResult>();
    }

    // ── DisableUser ───────────────────────────────────────────────────────────

    [Fact]
    public async Task DisableUser_WhenUserNotFound_ReturnsNotFound()
    {
        _userManager.Setup(m => m.FindByIdAsync("ghost")).ReturnsAsync((User?)null);

        var result = await _controller.DisableUser("ghost");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DisableUser_WhenUserFound_SetsLockoutEndToMaxValueAndReturnsOk()
    {
        var user = BuildUser();
        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.DisableUser(user.Id);

        result.Should().BeOfType<OkResult>();
        user.LockoutEnd.Should().Be(DateTimeOffset.MaxValue);
        _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }

    // ── EnableUser ────────────────────────────────────────────────────────────

    [Fact]
    public async Task EnableUser_WhenUserNotFound_ReturnsNotFound()
    {
        _userManager.Setup(m => m.FindByIdAsync("ghost")).ReturnsAsync((User?)null);

        var result = await _controller.EnableUser("ghost");

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task EnableUser_WhenUserFound_SetsLockoutEndToNullAndReturnsOk()
    {
        var user = BuildUser();
        user.LockoutEnd = DateTimeOffset.MaxValue; // pre-disabled
        _userManager.Setup(m => m.FindByIdAsync(user.Id)).ReturnsAsync(user);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _controller.EnableUser(user.Id);

        result.Should().BeOfType<OkResult>();
        user.LockoutEnd.Should().BeNull();
        _userManager.Verify(m => m.UpdateAsync(user), Times.Once);
    }
}
