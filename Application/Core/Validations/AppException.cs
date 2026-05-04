namespace Application.Core.Validations;

public class AppException(int statusCode, string message, string? details = null) : Exception
{
    public int StatusCode { get; } = statusCode;
    public new string Message { get; } = message;
    public string? Details { get; } = details;
}

