using Domain.ProductAggregate;

namespace Application.CustomProducts.DTOs;

public record SubmitCustomProductRequest
{
    public string TaskId { get; init; } = string.Empty;
    public string GlbUrl { get; init; } = string.Empty;
    public string RawDescription { get; init; } = string.Empty;
    public CustomProductDesignRequest Design { get; init; } = new();

    public CustomProduct ToEntity(string ownerUserId)
    {
        var name = string.IsNullOrWhiteSpace(Design.Type) ? "Custom Product" : Design.Type;
        return new CustomProduct
        {
            Name = name,
            Description = RawDescription,
            PictureUrl = GlbUrl,
            TaskId = TaskId,
            GlbUrl = GlbUrl,
            RawDescription = RawDescription,
            OwnerUserId = ownerUserId,
            Design = Design.ToEntity()
        };
    }
}