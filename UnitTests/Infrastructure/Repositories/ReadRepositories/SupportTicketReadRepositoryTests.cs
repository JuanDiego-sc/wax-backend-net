using Infrastructure.Repositories.ReadRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.ReadModels;

namespace UnitTests.Infrastructure.Repositories.ReadRepositories;

public class SupportTicketReadRepositoryTests
{
    private static ReadDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ReadDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ReadDbContext(options);
    }

    private static SupportTicketReadModel CreateTicketReadModel(string? id = null) => new()
    {
        Id = id ?? Guid.NewGuid().ToString(),
        UserId = "user-123",
        UserEmail = "user@example.com",
        UserFullName = "Test User",
        OrderId = "order-123",
        Category = "General",
        Status = "Open",
        Subject = "Test Subject",
        Description = "Test Description",
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async Task GetByIdAsync_WhenTicketExists_ReturnsDto()
    {
        using var context = CreateInMemoryContext();
        var ticketId = Guid.NewGuid().ToString();
        context.SupportTickets.Add(CreateTicketReadModel(ticketId));
        await context.SaveChangesAsync();

        var repository = new SupportTicketReadRepository(context);

        var result = await repository.GetByIdAsync(ticketId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(ticketId);
        result.UserEmail.Should().Be("user@example.com");
        result.Subject.Should().Be("Test Subject");
    }

    [Fact]
    public async Task GetByIdAsync_WhenTicketDoesNotExist_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var repository = new SupportTicketReadRepository(context);

        var result = await repository.GetByIdAsync("non-existent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetSupportTickets_ReturnsAllTicketsAsDtos()
    {
        using var context = CreateInMemoryContext();
        context.SupportTickets.Add(CreateTicketReadModel());
        context.SupportTickets.Add(CreateTicketReadModel());
        context.SupportTickets.Add(CreateTicketReadModel());
        await context.SaveChangesAsync();

        var repository = new SupportTicketReadRepository(context);

        var result = await repository.GetSupportTickets().ToListAsync();

        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetSupportTickets_MapsAllFieldsCorrectly()
    {
        using var context = CreateInMemoryContext();
        var ticketId = Guid.NewGuid().ToString();
        context.SupportTickets.Add(new SupportTicketReadModel
        {
            Id = ticketId,
            UserId = "user-456",
            UserEmail = "specific@example.com",
            UserFullName = "Specific User",
            OrderId = "order-456",
            Category = "Billing",
            Status = "InProgress",
            Subject = "Specific Subject",
            Description = "Specific Description",
            CreatedAt = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)
        });
        await context.SaveChangesAsync();

        var repository = new SupportTicketReadRepository(context);

        var result = await repository.GetSupportTickets().FirstAsync(t => t.Id == ticketId);

        result.UserId.Should().Be("user-456");
        result.UserEmail.Should().Be("specific@example.com");
        result.UserFullName.Should().Be("Specific User");
        result.OrderId.Should().Be("order-456");
        result.Category.Should().Be("Billing");
        result.Status.Should().Be("InProgress");
        result.Subject.Should().Be("Specific Subject");
        result.Description.Should().Be("Specific Description");
    }

    [Fact]
    public async Task GetSupportTickets_ReturnsQueryable_AllowsFiltering()
    {
        using var context = CreateInMemoryContext();
        var openTicket = CreateTicketReadModel();
        openTicket.Status = "Open";
        var closedTicket = CreateTicketReadModel();
        closedTicket.Status = "Closed";
        context.SupportTickets.Add(openTicket);
        context.SupportTickets.Add(closedTicket);
        await context.SaveChangesAsync();

        var repository = new SupportTicketReadRepository(context);

        var result = await repository.GetSupportTickets()
            .Where(t => t.Status == "Open")
            .ToListAsync();

        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Open");
    }

    [Fact]
    public async Task GetByIdAsync_SupportsCancellationToken()
    {
        using var context = CreateInMemoryContext();
        var ticketId = Guid.NewGuid().ToString();
        context.SupportTickets.Add(CreateTicketReadModel(ticketId));
        await context.SaveChangesAsync();

        var repository = new SupportTicketReadRepository(context);
        var cts = new CancellationTokenSource();

        var result = await repository.GetByIdAsync(ticketId, cts.Token);

        result.Should().NotBeNull();
    }
}
