using Application.Core;
using Application.Core.Validations;

namespace UnitTests.Application.Core;

public class ResultTests
{
    [Fact]
    public void Success_SetsIsSuccessTrue_AndStoresValue()
    {
        var result = Result<string>.Success("hello");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_SetsIsSuccessFalse_AndStoresError()
    {
        var result = Result<string>.Failure("something went wrong");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("something went wrong");
        result.Value.Should().BeNull();
    }

    [Fact]
    public void Failure_WithExplicitCode_StoresCode()
    {
        var result = Result<string>.Failure("not found", 404);

        result.Code.Should().Be(404);
    }

    [Fact]
    public void Failure_WithoutExplicitCode_DefaultsTo400()
    {
        var result = Result<string>.Failure("bad request");

        result.Code.Should().Be(400);
    }
}
