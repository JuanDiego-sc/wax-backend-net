namespace Infrastructure.Email;

public class EmailSettings
{
    public required string ApiToken { get; set; }
    public required string FromEmail { get; set; }
    public required string FromName { get; set; }
    public required string BaseUrl { get; set; }
}