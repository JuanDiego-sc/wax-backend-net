using System.Configuration;
using System.Runtime.InteropServices;
using Application.Interfaces;
using Application.Interfaces.DTOs;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using ImageUploadResultDto = Application.Interfaces.DTOs.ImageUploadResult;

namespace Infrastructure.Images;

public class ImageService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public ImageService(IOptions<CloudinarySettings> configuration)
    {
        var account = new Account(
            configuration.Value.CloudName,
            configuration.Value.ApiKey,
            configuration.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> DeleteImage(string publicId, CancellationToken cancellationToken = default)
    {
        var deleteParams = new DeletionParams(publicId);

        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Error != null) throw new ExternalException(result.Error.Message);

        return result.Result;
    }

    public async Task<ImageUploadResultDto?> UploadImage(ImageUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Content.Length < 0) return null;

        await using var stream = request.Content;

        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(request.FileName, stream),
            Folder = "WaxImages"
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null) throw new ExternalException(uploadResult.Error.Message);

        return new ImageUploadResultDto
        {
            PublicId = uploadResult.PublicId,
            Url = uploadResult.SecureUrl.ToString()
        };
    }
}