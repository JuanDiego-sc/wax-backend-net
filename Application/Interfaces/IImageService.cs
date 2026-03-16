using Application.Interfaces.DTOs;

namespace Application.Interfaces;

public interface IImageService
{
    Task<ImageUploadResult?> UploadImage(ImageUploadRequest request, CancellationToken cancellationToken = default);
    Task<string> DeleteImage(string publicId, CancellationToken cancellationToken = default);
}
