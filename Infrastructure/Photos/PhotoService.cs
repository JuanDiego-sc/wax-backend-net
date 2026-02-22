using System;
using Application.DTOs;
using Application.Interfaces;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Photos;

public class PhotoService : IPhotoService
{
    public readonly Cloudinary _cloudinary;

    public PhotoService(IOptions<CloudinarySettings> configuration)
    {
        var account = new Account(
            configuration.Value.CloudName,
            configuration.Value.ApiKey,
            configuration.Value.ApiSecret
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> DeletePhoto(string publicId)
    {
        var deleteParams = new DeletionParams(publicId);

        var result = await _cloudinary.DestroyAsync(deleteParams);

        if(result.Error != null)
        {
            throw new Exception(result.Error.Message);
        }

        return result.Result;
    }

    public async Task<PhotoUploadResult?> UploadPhoto(IFormFile file)
    {
        if(file.Length > 0 )
        {
            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "WaxImages"
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if(uploadResult.Error != null)
            {
                throw new Exception(uploadResult.Error.Message);
            }

            return new PhotoUploadResult
            {
                PublicId = uploadResult.PublicId,
                Url = uploadResult.SecureUrl.ToString()
            };
        }
        return null;
    }
}
