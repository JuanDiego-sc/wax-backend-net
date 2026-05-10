using Application.CustomProducts.DTOs;
using Domain.ProductAggregate;

namespace UnitTests.Helpers.Fixtures;

public static class CustomProductFixtures
{
    public static CustomProduct CreateCustomProduct(
        string? id = null,
        string ownerUserId = "user-1",
        string? basketId = null,
        CustomProductStatus status = CustomProductStatus.AwaitingAdminReview)
    {
        var product = new CustomProduct
        {
            Name = "Custom Ring",
            Description = "A custom 3D ring",
            PictureUrl = "https://cdn/model.glb",
            TaskId = id ?? Guid.NewGuid().ToString(),
            GlbUrl = "https://cdn/model.glb",
            RawDescription = "raw",
            OwnerUserId = ownerUserId,
            BasketId = basketId,
            Status = status,
            Design = new CustomProductDesign
            {
                Type = "Ring",
                Material = "Resin",
                Color = "Blue",
                Shape = "Round",
                Dimensions = "10x10x5"
            },
            Price = 5000
        };
        if (id != null) product.Id = id;
        return product;
    }

    public static CustomProduct WithSystemQuotation(this CustomProduct product, long amount = 5000)
    {
        product.Proposals.Add(new PriceProposal
        {
            Id = Guid.NewGuid().ToString(),
            CustomProductId = product.Id,
            Amount = amount,
            Source = ProposalSource.System
        });
        product.Status = CustomProductStatus.AwaitingAdminReview;
        product.Price = amount;
        return product;
    }

    public static CustomProduct WithAdminProposal(this CustomProduct product, long amount = 4500)
    {
        product.Proposals.Add(new PriceProposal
        {
            Id = Guid.NewGuid().ToString(),
            CustomProductId = product.Id,
            Amount = amount,
            Source = ProposalSource.Admin
        });
        product.Status = CustomProductStatus.AwaitingCustomerReview;
        product.Price = amount;
        return product;
    }

    public static CustomProduct WithCustomerCounterOffer(this CustomProduct product, long amount = 4000)
    {
        product.Proposals.Add(new PriceProposal
        {
            Id = Guid.NewGuid().ToString(),
            CustomProductId = product.Id,
            Amount = amount,
            Source = ProposalSource.Customer
        });
        product.Status = CustomProductStatus.AwaitingAdminReview;
        product.Price = amount;
        return product;
    }

    public static SubmitCustomProductRequest BuildSubmitRequest(string? taskId = null) =>
        new()
        {
            TaskId = taskId ?? Guid.NewGuid().ToString(),
            GlbUrl = "https://cdn/model.glb",
            RawDescription = "A beautiful ring",
            Design = new CustomProductDesignRequest
            {
                Type = "Ring",
                Material = "Resin",
                Color = "Blue",
                Shape = "Round",
                Dimensions = "10x10x5"
            }
        };
}
