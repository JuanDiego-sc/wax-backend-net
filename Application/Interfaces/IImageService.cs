using System;
using Application.Interfaces.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces;

public interface IImageService
{
    Task<ImageUploadResult?> UploadImage(IFormFile file);
    Task<string> DeleteImage(string publicId);
}
