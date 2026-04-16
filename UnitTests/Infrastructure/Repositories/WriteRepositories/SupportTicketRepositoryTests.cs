using Domain.Entities;
using Domain.SupportAssistAggregate;
using Infrastructure.Repositories.WriteRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace UnitTests.Infrastructure.Repositories.WriteRepositories;

public class SupportTicketRepositoryTests
{
    private static WriteDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WriteDbContext(options);
    }

    private static User CreateUser(string? id = null) => new()
    {
        Id = id ?? Guid.NewGuid().ToString(),
        UserName = "testuser",
        Email = "test@example.com"
    };

    private static SupportTicket CreateTicket(string? id = null, User? user = null) => new()
    {
        Id = id ?? Guid.NewGuid().ToString(),
        Subject = "Test Subject",
        Description = "Test Description",
        Category = TicketCategory.Other,
        Status = TicketStatus.Open,
        UserId = user?.Id ?? Guid.NewGuid().ToString(),
        User = user!
    };

    [Fact]
    public async Task GetQueryable_ReturnsAllTickets()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        context.SupportTickets.Add(CreateTicket(user: user));
        context.SupportTickets.Add(CreateTicket(user: user));
        await context.SaveChangesAsync();

        var repository = new SupportTicketRepository(context);

        var result = await repository.GetQueryable().ToListAsync();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTicketByIdAsync_WhenTicketExists_ReturnsTicketWithUser()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        var ticketId = Guid.NewGuid().ToString();
        var ticket = CreateTicket(id: ticketId, user: user);
        context.SupportTickets.Add(ticket);
        await context.SaveChangesAsync();

        var repository = new SupportTicketRepository(context);

        var result = await repository.GetTicketByIdAsync(ticketId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(ticketId);
        result.User.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTicketByIdAsync_WhenTicketDoesNotExist_ReturnsNull()
    {
        using var context = CreateInMemoryContext();
        var repository = new SupportTicketRepository(context);

        var result = await repository.GetTicketByIdAsync("non-existent", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Add_AddsTicketToContext()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var repository = new SupportTicketRepository(context);
        var ticket = CreateTicket(user: user);

        repository.Add(ticket);
        await context.SaveChangesAsync();

        var stored = await context.SupportTickets.FindAsync(ticket.Id);
        stored.Should().NotBeNull();
        stored!.Subject.Should().Be("Test Subject");
    }

    [Fact]
    public async Task Remove_RemovesTicketFromContext()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        var ticket = CreateTicket(user: user);
        context.SupportTickets.Add(ticket);
        await context.SaveChangesAsync();

        var repository = new SupportTicketRepository(context);

        repository.Remove(ticket);
        await context.SaveChangesAsync();

        var stored = await context.SupportTickets.FindAsync(ticket.Id);
        stored.Should().BeNull();
    }

    [Fact]
    public async Task GetQueryable_IncludesUserNavigation()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        var ticket = CreateTicket(user: user);
        context.SupportTickets.Add(ticket);
        await context.SaveChangesAsync();

        var repository = new SupportTicketRepository(context);

        var result = await repository.GetQueryable().FirstAsync();

        result.User.Should().NotBeNull();
        result.User!.UserName.Should().Be("testuser");
    }
}
