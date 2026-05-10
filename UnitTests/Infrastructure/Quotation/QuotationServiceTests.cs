using Application.Interfaces.DTOs;
using Application.Interfaces.Services;
using Domain.ProductAggregate;
using Infrastructure.Quotation;

namespace UnitTests.Infrastructure.Quotation;

public class QuotationServiceTests
{
    private readonly Mock<IQuotationRulesCache> _rulesMock = new();
    private readonly Mock<IDimensionParser> _parserMock = new();
    private readonly QuotationService _service;

    public QuotationServiceTests()
    {
        _service = new QuotationService(_rulesMock.Object, _parserMock.Object);
        
        // Setup default rules
        _rulesMock.Setup(r => r.GetAsync("DEFAULT_DEPTH_CM", 5m, It.IsAny<CancellationToken>())).ReturnsAsync(5m);
        _rulesMock.Setup(r => r.GetAsync("BASE_COST", 5000m, It.IsAny<CancellationToken>())).ReturnsAsync(5000m);
        _rulesMock.Setup(r => r.GetAsync("MARGIN_MULTIPLIER", 1.6m, It.IsAny<CancellationToken>())).ReturnsAsync(1.6m);
        _rulesMock.Setup(r => r.GetAsync("MATERIAL_default", 2m, It.IsAny<CancellationToken>())).ReturnsAsync(2m);
    }

    [Fact]
    public async Task QuoteAsync_WithUnparsedDimensions_UsesDefaultsAndCalculatesCorrectly()
    {
        // Arrange
        var design = new CustomProductDesign 
        { 
            Material = "raw material", 
            Type = "neon sign", 
            Dimensions = "unknown dimensions",
            Color = "Red",
            Shape = "Square"
        };
        var parsed = new ParsedDimensions(0, 0, null);
        _parserMock.Setup(p => p.TryParse("unknown dimensions", out parsed)).Returns(false);

        _rulesMock.Setup(r => r.GetAsync("MATERIAL_raw_material", 2m, It.IsAny<CancellationToken>())).ReturnsAsync(2m);
        
        // base cost: 5000, margin: 1.6, volume is 0 (width and height remain 0 because parse failed)
        // subtotal = 0 * 2 + 5000 = 5000
        // total = 5000 * 1.6 = 8000

        // Act
        var result = await _service.QuoteAsync(design, CancellationToken.None);

        // Assert
        result.Amount.Should().Be(8000);
        result.Breakdown["volume_cm3"].Should().Be(0);
        result.Breakdown["material_unit_cents_cm3"].Should().Be(2m);
    }

    [Fact]
    public async Task QuoteAsync_WithParsedDimensionsWithoutDepth_UsesTypeDepthAndCalculatesCorrectly()
    {
        // Arrange
        var design = new CustomProductDesign 
        { 
            Material = "acrylic", 
            Type = "neon_sign", 
            Dimensions = "10x20 cm",
            Color = "Blue",
            Shape = "Circle"
        };
        var parsed = new ParsedDimensions(10, 20, null);
        _parserMock.Setup(p => p.TryParse("10x20 cm", out parsed)).Returns(true);

        _rulesMock.Setup(r => r.GetAsync("MATERIAL_acrylic", 2m, It.IsAny<CancellationToken>())).ReturnsAsync(3m);
        _rulesMock.Setup(r => r.GetAsync("TYPE_neon_sign_DEFAULT_DEPTH", 5m, It.IsAny<CancellationToken>())).ReturnsAsync(4m);

        // depth = 4, width = 10, height = 20
        // volume = 10 * 20 * 4 = 800
        // material unit = 3
        // subtotal = 800 * 3 + 5000 = 7400
        // total = 7400 * 1.6 = 11840

        // Act
        var result = await _service.QuoteAsync(design, CancellationToken.None);

        // Assert
        result.Amount.Should().Be(11840);
        result.Breakdown["volume_cm3"].Should().Be(800);
        result.Breakdown["base_cost_cents"].Should().Be(5000);
    }
}
