using Application.Interfaces.DTOs;
using Infrastructure.Images;
using Microsoft.Extensions.Options;

namespace UnitTests.Infrastructure.Images;

public class ImageServiceTests
{
    private static ImageService CreateService()
    {
        var settings = new Mock<IOptions<CloudinarySettings>>();
        settings.Setup(s => s.Value).Returns(new CloudinarySettings
        {
            CloudName = "test-cloud",
            ApiKey = "test-key",
            ApiSecret = "test-secret"
        });
        return new ImageService(settings.Object);
    }

    [Fact]
    public void Constructor_WithValidSettings_DoesNotThrow()
    {
        var act = CreateService;

        act.Should().NotThrow();
    }

    [Fact]
    public async Task UploadImage_WhenContentLengthIsNegative_ReturnsNull()
    {
        var service = CreateService();
        var mockStream = new Mock<Stream>();
        mockStream.Setup(s => s.Length).Returns(-1L);

        var request = new ImageUploadRequest(mockStream.Object, "test.jpg", "image/jpeg");

        var result = await service.UploadImage(request);

        result.Should().BeNull();
    }
}
