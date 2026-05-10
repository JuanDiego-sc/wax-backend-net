using Application.IntegrationEvents.CustomProductEvents;
using Domain.ProductAggregate;
using Infrastructure.Messaging.Consumers.CustomProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.CustomProductConsumers;

public class CustomProductRejectedConsumerTests
{
    private static ReadDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ConsumeContext<CustomProductRejectedIntegrationEvent> BuildContext(
        CustomProductRejectedIntegrationEvent evt)
    {
        var mock = new Mock<ConsumeContext<CustomProductRejectedIntegrationEvent>>();
        mock.Setup(c => c.Message).Returns(evt);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock.Object;
    }

    private static void SeedReadModel(ReadDbContext ctx, string id)
    {
        ctx.CustomProducts.Add(new CustomProductReadModel
        {
            Id = id,
            Name = "Ring",
            Description = "Desc",
            Price = 5000,
            PictureUrl = "url",
            TaskId = Guid.NewGuid().ToString(),
            GlbUrl = "url",
            OwnerUserId = "user-1",
            Status = "AwaitingAdminReview",
            DesignType = "Ring",
            DesignMaterial = "Resin",
            DesignColor = "Blue",
            DesignShape = "Round",
            DesignDimensions = "5x5x5",
            CreatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();
    }

    [Fact]
    public async Task Consume_WhenFound_SetsStatusToRejected()
    {
        using var ctx = CreateContext();
        SeedReadModel(ctx, "cp-rej");
        var consumer = new CustomProductRejectedConsumer(ctx, Mock.Of<ILogger<CustomProductRejectedConsumer>>());
        var evt = new CustomProductRejectedIntegrationEvent
        {
            CustomProductId = "cp-rej",
            OwnerUserId = "user-1",
            Reason = "Price too high",
            OccurredAt = DateTime.UtcNow
        };

        await consumer.Consume(BuildContext(evt));

        var model = await ctx.CustomProducts.FirstAsync(p => p.Id == "cp-rej");
        model.Status.Should().Be(CustomProductStatus.Rejected.ToString());
        model.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Consume_WhenNotFound_DoesNotThrow()
    {
        using var ctx = CreateContext();
        var consumer = new CustomProductRejectedConsumer(ctx, Mock.Of<ILogger<CustomProductRejectedConsumer>>());
        var evt = new CustomProductRejectedIntegrationEvent
        {
            CustomProductId = "missing",
            OwnerUserId = "user-1",
            Reason = "reason"
        };

        var act = () => consumer.Consume(BuildContext(evt));

        await act.Should().NotThrowAsync();
    }
}
