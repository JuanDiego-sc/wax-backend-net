using Application.IntegrationEvents.SupportTicketEvents;
using Infrastructure.Messaging.Consumers.SupportTicketConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.SupportTicketConsumers;

public class SupportTicketDeletedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static ILogger<SupportTicketDeletedConsumer> CreateLogger()
    {
        var logger = new Mock<ILogger<SupportTicketDeletedConsumer>>();
        return logger.Object;
    }

    private static SupportTicketReadModel CreateTicketReadModel(string ticketId) => new()
    {
        Id = ticketId,
        UserId = "user-1",
        UserEmail = "user@test.com",
        UserFullName = "Test User",
        OrderId = "order-1",
        Category = "PaymentIssue",
        Status = "Open",
        Subject = "Subject",
        Description = "Description",
        CreatedAt = DateTime.UtcNow,
        LastSyncedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Consume_WhenTicketExists_RemovesTicket()
    {
        using var context = CreateInMemoryContext();
        var ticketId = Guid.NewGuid().ToString();
        context.SupportTickets.Add(CreateTicketReadModel(ticketId));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new SupportTicketDeletedConsumer(context, logger);
        var @event = new SupportTicketDeletedIntegrationEvent { TicketId = ticketId };

        var contextMock = new Mock<ConsumeContext<SupportTicketDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var ticket = await context.SupportTickets.FirstOrDefaultAsync(t => t.Id == ticketId);
        ticket.Should().BeNull();
    }

    [Fact]
    public async Task Consume_WhenTicketNotFound_DoesNotThrow()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new SupportTicketDeletedConsumer(context, logger);
        var @event = new SupportTicketDeletedIntegrationEvent { TicketId = "non-existent" };

        var contextMock = new Mock<ConsumeContext<SupportTicketDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        var act = async () => await consumer.Consume(contextMock.Object);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Consume_OnlyRemovesSpecifiedTicket()
    {
        using var context = CreateInMemoryContext();
        var ticketToDelete = Guid.NewGuid().ToString();
        var ticketToKeep = Guid.NewGuid().ToString();
        context.SupportTickets.Add(CreateTicketReadModel(ticketToDelete));
        context.SupportTickets.Add(CreateTicketReadModel(ticketToKeep));
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new SupportTicketDeletedConsumer(context, logger);
        var @event = new SupportTicketDeletedIntegrationEvent { TicketId = ticketToDelete };

        var contextMock = new Mock<ConsumeContext<SupportTicketDeletedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var remainingTickets = await context.SupportTickets.ToListAsync();
        remainingTickets.Should().HaveCount(1);
        remainingTickets[0].Id.Should().Be(ticketToKeep);
    }
}
