namespace Application.CustomProducts.DTOs;

public record CustomProductDto{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public long Price { get; init; }
    public required string TaskId { get; init; }
    public required string GlbUrl { get; init; }
    public required string OwnerUserId { get; init; }
    public required string Status { get; init; }
    public long? AgreedPrice { get; init; }
    public required CustomProductDesignRequest Design { get; init; }
    public List<PriceProposalResponse> Proposals { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}