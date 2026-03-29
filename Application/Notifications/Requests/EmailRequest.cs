namespace Application.Notifications.Requests;

public abstract record EmailRequest
{
    public required string ToEmail { get; init; }
    public required string ToName { get; init; }
    public abstract EmailType EmailType { get; }
    
}