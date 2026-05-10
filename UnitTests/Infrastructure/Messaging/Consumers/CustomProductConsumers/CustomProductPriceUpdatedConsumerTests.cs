using Application.IntegrationEvents.CustomProductEvents;
using Domain.ProductAggregate;
using Infrastructure.Messaging.Consumers.CustomProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.CustomProductConsumers;

public class CustomProductPriceUpdatedConsumerTests
{
    private static ReadDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ConsumeContext<CustomProductPriceUpdatedIntegrationEvent> BuildContext(
        CustomProductPriceUpdatedIntegrationEvent evt)
    {
        var mock = new Mock<ConsumeContext<CustomProductPriceUpdatedIntegrationEvent>>();
        mock.Setup(c => c.Message).Returns(evt);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock.Object;
    }

    private static CustomProductReadModel SeedReadModel(ReadDbContext ctx, string id = "cp-1",
        string status = "AwaitingCustomerReview")
    {
        var model = new CustomProductReadModel
        {
            Id = id,
            Name = "Ring",
            Description = "Desc",
            Price = 5000,
            PictureUrl = "url",
            TaskId = Guid.NewGuid().ToString(),
            GlbUrl = "url",
            OwnerUserId = "user-1",
            Status = status,
            DesignType = "Ring",
            DesignMaterial = "Resin",
            DesignColor = "Blue",
            DesignShape = "Round",
            DesignDimensions = "5x5x5",
            CreatedAt = DateTime.UtcNow
        };
        ctx.CustomProducts.Add(model);
        ctx.SaveChanges();
        return model;
    }

    [Fact]
    public async Task Consume_WhenFound_UpdatesPriceAndStatus()
    {
        using var ctx = CreateContext();
        SeedReadModel(ctx, "cp-upd");
        var consumer = new CustomProductPriceUpdatedConsumer(ctx, Mock.Of<ILogger<CustomProductPriceUpdatedConsumer>>());
        var evt = new CustomProductPriceUpdatedIntegrationEvent
        {
            CustomProductId = "cp-upd",
            OwnerUserId = "user-1",
            Status = "AwaitingAdminReview",
            Price = 4000,
            ProposalId = "prop-1",
            ProposalSource = "Customer",
            OccurredAt = DateTime.UtcNow
        };

        await consumer.Consume(BuildContext(evt));

        var model = await ctx.CustomProducts.FirstAsync(p => p.Id == "cp-upd");
        model.Price.Should().Be(4000);
        model.Status.Should().Be("AwaitingAdminReview");
        model.UpdatedAt.Should().NotBeNull();
        model.AgreedPrice.Should().BeNull();
    }

    [Fact]
    public async Task Consume_WhenStatusIsApproved_SetsAgreedPrice()
    {
        using var ctx = CreateContext();
        SeedReadModel(ctx, "cp-appr");
        var consumer = new CustomProductPriceUpdatedConsumer(ctx, Mock.Of<ILogger<CustomProductPriceUpdatedConsumer>>());
        var evt = new CustomProductPriceUpdatedIntegrationEvent
        {
            CustomProductId = "cp-appr",
            OwnerUserId = "user-1",
            Status = CustomProductStatus.Approved.ToString(),
            Price = 4500,
            ProposalId = "prop-1",
            ProposalSource = "Admin",
            OccurredAt = DateTime.UtcNow
        };

        await consumer.Consume(BuildContext(evt));

        var model = await ctx.CustomProducts.FirstAsync(p => p.Id == "cp-appr");
        model.AgreedPrice.Should().Be(4500);
    }

    [Fact]
    public async Task Consume_WhenNotFound_DoesNotThrow()
    {
        using var ctx = CreateContext();
        var consumer = new CustomProductPriceUpdatedConsumer(ctx, Mock.Of<ILogger<CustomProductPriceUpdatedConsumer>>());
        var evt = new CustomProductPriceUpdatedIntegrationEvent
        {
            CustomProductId = "missing",
            OwnerUserId = "user-1",
            Status = "AwaitingAdminReview",
            Price = 3000,
            ProposalId = "p",
            ProposalSource = "Customer"
        };

        var act = () => consumer.Consume(BuildContext(evt));

        await act.Should().NotThrowAsync();
        ctx.CustomProducts.Should().BeEmpty();
    }
}
