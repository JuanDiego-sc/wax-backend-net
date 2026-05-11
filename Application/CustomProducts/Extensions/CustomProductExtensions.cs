using Application.CustomProducts.DTOs;
using Domain.ProductAggregate;

namespace Application.CustomProducts.Extensions;

public static class CustomProductExtensions
{
    public static CustomProductDto ToDto(this CustomProduct product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        Description = product.Description,
        Price = product.Price,
        TaskId = product.TaskId,
        GlbUrl = product.GlbUrl,
        OwnerUserId = product.OwnerUserId,
        Status = product.Status.ToString(),
        AgreedPrice = product.AgreedPrice,
        CreatedAt = product.CreatedAt,
        UpdatedAt = product.UpdatedAt,
        Design = new CustomProductDesignRequest
        {
            Type = product.Design.Type,
            Material = product.Design.Material,
            Color = product.Design.Color,
            Shape = product.Design.Shape,
            Dimensions = product.Design.Dimensions,
            Details = product.Design.Details
        },
        Proposals = product.Proposals
            .OrderBy(p => p.CreatedAt)
            .Select(p => new PriceProposalResponse
            {
                Id = p.Id,
                Amount = p.Amount,
                Source = p.Source.ToString(),
                Comment = p.Comment,
                IsAccepted = p.IsAccepted,
                CreatedAt = p.CreatedAt
            })
            .ToList()
    };
}