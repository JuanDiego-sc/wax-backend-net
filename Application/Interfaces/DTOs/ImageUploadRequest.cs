namespace Application.Interfaces.DTOs;

public record ImageUploadRequest(Stream Content, string FileName, string ContentType);