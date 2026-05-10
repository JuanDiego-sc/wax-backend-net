using Application.IntegrationEvents.CustomProductEvents;
using Domain.ProductAggregate;
using Infrastructure.Messaging.Consumers.CustomProductConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.CustomProductConsumers;

public class CustomProductSubmittedConsumerTests
{
    private static ReadDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

    private static ConsumeContext<CustomProductSubmittedIntegrationEvent> BuildContext(
        CustomProductSubmittedIntegrationEvent evt)
    {
        var mock = new Mock<ConsumeContext<CustomProductSubmittedIntegrationEvent>>();
        mock.Setup(c => c.Message).Returns(evt);
        mock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return mock.Object;
    }

    private static CustomProductSubmittedIntegrationEvent BuildEvent(string id = "cp-1") => new()
    {
        CustomProductId = id,
        TaskId = "task-1",
        OwnerUserId = "user-1",
        Name = "Custom Ring",
        Description = "A test ring",
        QuotedPrice = 5000,
        GlbUrl = "https://cdn/model.glb",
        DesignType = "Ring",
        DesignMaterial = "Resin",
        DesignColor = "Blue",
        DesignShape = "Round",
        DesignDimensions = "10x10x5",
        DesignDetails = "special edition",
        OccurredAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Consume_WhenNew_ProjectsReadModel()
    {
        using var ctx = CreateContext();
        var consumer = new CustomProductSubmittedConsumer(ctx, Mock.Of<ILogger<CustomProductSubmittedConsumer>>());
        var evt = BuildEvent("new-cp");

        await consumer.Consume(BuildContext(evt));

        var model = await ctx.CustomProducts.FirstOrDefaultAsync(p => p.Id == "new-cp");
        model.Should().NotBeNull();
        model!.Name.Should().Be("Custom Ring");
        model.Description.Should().Be("A test ring");
        model.Price.Should().Be(5000);
        model.TaskId.Should().Be("task-1");
        model.GlbUrl.Should().Be("https://cdn/model.glb");
        model.PictureUrl.Should().Be("https://cdn/model.glb");
        model.OwnerUserId.Should().Be("user-1");
        model.Status.Should().Be(CustomProductStatus.AwaitingAdminReview.ToString());
        model.DesignType.Should().Be("Ring");
        model.DesignMaterial.Should().Be("Resin");
        model.DesignColor.Should().Be("Blue");
        model.DesignShape.Should().Be("Round");
        model.DesignDimensions.Should().Be("10x10x5");
        model.DesignDetails.Should().Be("special edition");
        model.AgreedPrice.Should().BeNull();
    }

    [Fact]
    public async Task Consume_WhenAlreadyExists_IsIdempotent()
    {
        using var ctx = CreateContext();
        ctx.CustomProducts.Add(new CustomProductReadModel
        {
            Id = "dup-cp",
            Name = "Original",
            Description = "Desc",
            Price = 1000,
            PictureUrl = "url",
            TaskId = "t1",
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
        await ctx.SaveChangesAsync();

        var consumer = new CustomProductSubmittedConsumer(ctx, Mock.Of<ILogger<CustomProductSubmittedConsumer>>());
        await consumer.Consume(BuildContext(BuildEvent("dup-cp")));

        var models = await ctx.CustomProducts.Where(p => p.Id == "dup-cp").ToListAsync();
        models.Should().HaveCount(1);
        models[0].Name.Should().Be("Original");
    }

    [Fact]
    public async Task Consume_WhenNameEmpty_FallsBackToDesignType()
    {
        using var ctx = CreateContext();
        var consumer = new CustomProductSubmittedConsumer(ctx, Mock.Of<ILogger<CustomProductSubmittedConsumer>>());
        var evt = BuildEvent("fallback-cp");
        evt.Name = "";

        await consumer.Consume(BuildContext(evt));

        var model = await ctx.CustomProducts.FirstOrDefaultAsync(p => p.Id == "fallback-cp");
        model!.Name.Should().Be("Ring");
    }
}
