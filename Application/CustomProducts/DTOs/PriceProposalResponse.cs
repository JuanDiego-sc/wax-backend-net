namespace Application.CustomProducts.DTOs;

public record PriceProposalResponse{
    public required string Id { get; init; }
    public long Amount { get; init; }
    public required string Source { get; init; }
    public string? Comment { get; init; }
    public bool IsAccepted { get; init; }
    public DateTime CreatedAt { get; init; }
}