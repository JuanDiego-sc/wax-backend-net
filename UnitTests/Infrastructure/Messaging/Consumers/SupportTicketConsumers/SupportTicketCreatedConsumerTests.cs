using Application.IntegrationEvents.SupportTicketEvents;
using Infrastructure.Messaging.Consumers.SupportTicketConsumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Messaging.Consumers.SupportTicketConsumers;

public class SupportTicketCreatedConsumerTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static ILogger<SupportTicketCreatedConsumer> CreateLogger()
    {
        var logger = new Mock<ILogger<SupportTicketCreatedConsumer>>();
        return logger.Object;
    }

    private static SupportTicketCreatedIntegrationEvent CreateEvent(string? ticketId = null) => new()
    {
        TicketId = ticketId ?? Guid.NewGuid().ToString(),
        UserId = Guid.NewGuid().ToString(),
        UserEmail = "user@test.com",
        UserFullName = "Test User",
        OrderId = Guid.NewGuid().ToString(),
        Category = "PaymentIssue",
        Status = "Open",
        Subject = "Test Subject",
        Description = "Test Description",
        OccurredAt = DateTime.UtcNow
    };

    [Fact]
    public async Task Consume_WhenTicketDoesNotExist_AddsNewTicketReadModel()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new SupportTicketCreatedConsumer(context, logger);
        var @event = CreateEvent(ticketId: "new-ticket");

        var contextMock = new Mock<ConsumeContext<SupportTicketCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var ticket = await context.SupportTickets.FirstOrDefaultAsync(t => t.Id == "new-ticket");
        ticket.Should().NotBeNull();
        ticket!.UserEmail.Should().Be("user@test.com");
        ticket.Category.Should().Be("PaymentIssue");
        ticket.Status.Should().Be("Open");
        ticket.Subject.Should().Be("Test Subject");
    }

    [Fact]
    public async Task Consume_WhenTicketAlreadyExists_DoesNotAddDuplicate()
    {
        using var context = CreateInMemoryContext();
        var existingTicket = new SupportTicketReadModel
        {
            Id = "existing-ticket",
            UserId = "user-1",
            UserEmail = "original@test.com",
            UserFullName = "Original User",
            OrderId = "order-1",
            Category = "Other",
            Status = "Closed",
            Subject = "Original Subject",
            Description = "Original Description",
            CreatedAt = DateTime.UtcNow,
            LastSyncedAt = DateTime.UtcNow
        };
        context.SupportTickets.Add(existingTicket);
        await context.SaveChangesAsync();

        var logger = CreateLogger();
        var consumer = new SupportTicketCreatedConsumer(context, logger);
        var @event = CreateEvent(ticketId: "existing-ticket");

        var contextMock = new Mock<ConsumeContext<SupportTicketCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var tickets = await context.SupportTickets.Where(t => t.Id == "existing-ticket").ToListAsync();
        tickets.Should().HaveCount(1);
        tickets[0].UserEmail.Should().Be("original@test.com");
    }

    [Fact]
    public async Task Consume_MapsAllFieldsCorrectly()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new SupportTicketCreatedConsumer(context, logger);
        var @event = CreateEvent();

        var contextMock = new Mock<ConsumeContext<SupportTicketCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var ticket = await context.SupportTickets.FirstOrDefaultAsync(t => t.Id == @event.TicketId);
        ticket.Should().NotBeNull();
        ticket!.UserId.Should().Be(@event.UserId);
        ticket.UserFullName.Should().Be("Test User");
        ticket.OrderId.Should().Be(@event.OrderId);
        ticket.Description.Should().Be("Test Description");
    }

    [Fact]
    public async Task Consume_SetsLastSyncedAt()
    {
        using var context = CreateInMemoryContext();
        var logger = CreateLogger();
        var consumer = new SupportTicketCreatedConsumer(context, logger);
        var @event = CreateEvent();

        var contextMock = new Mock<ConsumeContext<SupportTicketCreatedIntegrationEvent>>();
        contextMock.Setup(c => c.Message).Returns(@event);
        contextMock.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(contextMock.Object);

        var ticket = await context.SupportTickets.FirstOrDefaultAsync(t => t.Id == @event.TicketId);
        ticket!.LastSyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }
}
