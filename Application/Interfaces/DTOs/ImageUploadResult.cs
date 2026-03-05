using System;

namespace Application.Interfaces.DTOs;

public class ImageUploadResult
{
    public required string PublicId { get; set; }
    public required string Url { get; set; }
}
