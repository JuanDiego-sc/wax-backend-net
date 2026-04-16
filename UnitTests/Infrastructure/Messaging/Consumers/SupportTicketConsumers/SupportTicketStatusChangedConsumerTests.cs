using Application.IntegrationEvents.SupportTicketEvents;
using Infrastructure.Messaging.Consumers.SupportTicketConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.SupportTicketConsumers;

public class SupportTicketStatusChangedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static ILogger<SupportTicketStatusChangedConsumer> CreateLogger()
    {
        var logger = new Mock<ILogger<SupportTicketStatusChangedConsumer>>();
        return logger.Object;
    }

    private static SupportTicketReadModel CreateTicketReadModel(string ticketId, string status = "Open") => new()
    {
        Id = ticketId,
        UserId = "user-1",
        UserEmail = "user@test.com",
        UserFullName = "Test User",
        OrderId = "order-1",
        Category = "PaymentIssue",
        Status = status,
        Subject = "Subject",
        Description = "Description",
        CreatedAt = DateTime.UtcNow,
        LastSyncedAt = DateTime.UtcNow.AddMinutes(-5)
    };

    [Fact]
    public async Task Consume_WhenTicketExists_UpdatesStatus()
    {
        using var context = CreateInMemoryContext();
        var ticketId = Guid.NewGuid().ToString();
        context.SupportTickets.Add(CreateTicketReadModel(ticketId, "Open"));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new SupportTicketStatusChangedConsumer(context, logger);
        var @event = new SupportTicketStatusChangedIntegrationEvent
        {
            TicketId = ticketId,
            NewStatus = "Closed"
        };

        var contextMock = new Mock<ConsumeContext<SupportTicketStatusChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var ticket = await context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        ticket.Should().NotBeNull();
        ticket!.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task Consume_WhenTicketNotFound_ThrowsException()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new SupportTicketStatusChangedConsumer(context, logger);
        var @event = new SupportTicketStatusChangedIntegrationEvent
        {
            TicketId = "non-existent",
            NewStatus = "Closed"
        };

        var contextMock = new Mock<ConsumeContext<SupportTicketStatusChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(contextMock.Object);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Consume_UpdatesTimestamps()
    {
        using var context = CreateInMemoryContext();
        var ticketId = Guid.NewGuid().ToString();
        context.SupportTickets.Add(CreateTicketReadModel(ticketId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new SupportTicketStatusChangedConsumer(context, logger);
        var eventTime = DateTime.UtcNow;
        var @event = new SupportTicketStatusChangedIntegrationEvent
        {
            TicketId = ticketId,
            NewStatus = "InProgress",
            OccurredAt = eventTime
        };

        var contextMock = new Mock<ConsumeContext<SupportTicketStatusChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var ticket = await context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        ticket!.UpdatedAt.Should().BeCloseTo(eventTime, TimeSpan.FromSeconds(1));
        ticket.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public async Task Consume_DoesNotModifyOtherFields()
    {
        using var context = CreateInMemoryContext();
        var ticketId = Guid.NewGuid().ToString();
        context.SupportTickets.Add(CreateTicketReadModel(ticketId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new SupportTicketStatusChangedConsumer(context, logger);
        var @event = new SupportTicketStatusChangedIntegrationEvent
        {
            TicketId = ticketId,
            NewStatus = "Closed"
        };

        var contextMock = new Mock<ConsumeContext<SupportTicketStatusChangedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var ticket = await context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        ticket!.Category.Should().Be("PaymentIssue");
        ticket.Subject.Should().Be("Subject");
        ticket.Description.Should().Be("Description");
        ticket.UserId.Should().Be("user-1");
    }
}
