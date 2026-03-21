namespace Persistence.ReadModels;

public class SupportTicketReadModel
{
    public required string Id { get; set; }
    public required string UserId { get; set; }
    public required string UserEmail { get; set; }
    public required string UserFullName { get; set; }
    public required string OrderId { get; set; }
    public required string Category { get; set; }
    public required string Status { get; set; }
    public required string Subject { get; set; }
    public required string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime LastSyncedAt { get; set; }
}
