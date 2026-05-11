using Application.Interfaces.DTOs;
using Infrastructure.Quotation;

namespace UnitTests.Infrastructure.Quotation;

public class DimensionParserTests
{
    private readonly DimensionParser _parser = new();

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    [InlineData("10 x invalid cm")]
    [InlineData("cm")]
    [InlineData("10 x cm")]
    public void TryParse_InvalidInput_ReturnsFalse(string raw)
    {
        var result = _parser.TryParse(raw, out var dimensions);

        result.Should().BeFalse();
        dimensions.WidthCm.Should().Be(0);
        dimensions.HeightCm.Should().Be(0);
        dimensions.DepthCm.Should().BeNull();
    }

    [Theory]
    [InlineData("10x20cm", 10, 20, null)]
    [InlineData("10 x 20 cm", 10, 20, null)]
    [InlineData(" 10   x   20   cm ", 10, 20, null)]
    [InlineData("10x20x5cm", 10, 20, 5)]
    [InlineData("10 x 20 x 5 cm", 10, 20, 5)]
    [InlineData("10x20x0cm", 10, 20, 0)]
    public void TryParse_ValidInput_ReturnsParsedDimensions(string raw, double expectedWidth, double expectedHeight, object expectedDepth)
    {
        var result = _parser.TryParse(raw, out var dimensions);

        result.Should().BeTrue();
        dimensions.WidthCm.Should().Be((decimal)expectedWidth);
        dimensions.HeightCm.Should().Be((decimal)expectedHeight);
        
        if (expectedDepth == null)
            dimensions.DepthCm.Should().BeNull();
        else
            dimensions.DepthCm.Should().Be(Convert.ToDecimal(expectedDepth));
    }
}
