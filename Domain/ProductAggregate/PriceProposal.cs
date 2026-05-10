using Domain.Entities;

namespace Domain.ProductAggregate;

public class PriceProposal : BaseEntity
{
    public required string CustomProductId { get; set; }
    public long Amount { get; set; }
    public ProposalSource Source { get; set; }
    public string? Comment { get; set; }
    public bool IsAccepted { get; set; }
}