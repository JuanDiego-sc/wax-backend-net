using System.Security.Claims;
using Domain.Entities;
using Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace UnitTests.Infrastructure.Security;

public class UserAccessorTests
{
    private static WriteDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WriteDbContext(options);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(string? userId = null, string? email = null)
    {
        var claims = new List<Claim>();
        if (userId != null)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
        if (email != null)
            claims.Add(new Claim(ClaimTypes.Email, email));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns(httpContext);
        return accessor.Object;
    }

    private static IHttpContextAccessor CreateNullHttpContextAccessor()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.Setup(x => x.HttpContext).Returns((HttpContext?)null);
        return accessor.Object;
    }

    [Fact]
    public void GetUserId_WhenUserIdExists_ReturnsUserId()
    {
        using var context = CreateInMemoryContext();
        var userId = "user-123";
        var accessor = CreateHttpContextAccessor(userId: userId);
        var userAccessor = new UserAccessor(accessor, context);

        var result = userAccessor.GetUserId();

        result.Should().Be(userId);
    }

    [Fact]
    public void GetUserId_WhenHttpContextIsNull_ThrowsUnauthorizedAccessException()
    {
        using var context = CreateInMemoryContext();
        var accessor = CreateNullHttpContextAccessor();
        var userAccessor = new UserAccessor(accessor, context);

        var act = () => userAccessor.GetUserId();

        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("User ID not found.");
    }

    [Fact]
    public void GetUserEmail_WhenEmailExists_ReturnsEmail()
    {
        using var context = CreateInMemoryContext();
        var email = "user@example.com";
        var accessor = CreateHttpContextAccessor(email: email);
        var userAccessor = new UserAccessor(accessor, context);

        var result = userAccessor.GetUserEmail();

        result.Should().Be(email);
    }

    [Fact]
    public void GetUserEmail_WhenHttpContextIsNull_ThrowsUnauthorizedAccessException()
    {
        using var context = CreateInMemoryContext();
        var accessor = CreateNullHttpContextAccessor();
        var userAccessor = new UserAccessor(accessor, context);

        var act = () => userAccessor.GetUserEmail();

        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("User email not found.");
    }

    [Fact]
    public async Task GetUserAsync_WhenUserExists_ReturnsUser()
    {
        using var context = CreateInMemoryContext();
        var userId = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com"
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var accessor = CreateHttpContextAccessor(userId: userId);
        var userAccessor = new UserAccessor(accessor, context);

        var result = await userAccessor.GetUserAsync();

        result.Should().NotBeNull();
        result!.Id.Should().Be(userId);
        result.UserName.Should().Be("testuser");
    }

    [Fact]
    public async Task GetUserAsync_WhenUserDoesNotExist_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var accessor = CreateHttpContextAccessor(userId: "non-existent-user");
        var userAccessor = new UserAccessor(accessor, context);

        var result = await userAccessor.GetUserAsync();

        result.Should().BeNull();
    }

    [Fact]
    public void GetUserId_WhenUserIdClaimIsMissing_ThrowsUnauthorizedAccessException()
    {
        using var context = CreateInMemoryContext();
        var accessor = CreateHttpContextAccessor(email: "test@example.com");
        var userAccessor = new UserAccessor(accessor, context);

        var act = () => userAccessor.GetUserId();

        act.Should().Throw<UnauthorizedAccessException>();
    }
}
