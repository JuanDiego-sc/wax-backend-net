using Application.Core;
using Application.Core.Validations;

namespace UnitTests.Application.Core;

public class AppExceptionTests
{
    [Fact]
    public void Constructor_SetsAllProperties_Correctly()
    {
        var exception = new AppException(404, "Not found", "Some detail");

        exception.StatusCode.Should().Be(404);
        exception.Message.Should().Be("Not found");
        exception.Details.Should().Be("Some detail");
    }

    [Fact]
    public void Constructor_WhenDetailsNotProvided_DefaultsToNull()
    {
        var exception = new AppException(500, "Internal error");

        exception.StatusCode.Should().Be(500);
        exception.Message.Should().Be("Internal error");
        exception.Details.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithStatusCode400_SetsStatusCode()
    {
        var exception = new AppException(400, "Bad request", "Validation error details");

        exception.StatusCode.Should().Be(400);
    }
}
