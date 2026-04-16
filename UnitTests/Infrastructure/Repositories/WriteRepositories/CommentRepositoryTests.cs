using Domain.Entities;
using Domain.SupportAssistAggregate;
using Infrastructure.Repositories.WriteRepositories;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace UnitTests.Infrastructure.Repositories.WriteRepositories;

public class CommentRepositoryTests
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

    private static Comment CreateComment(string ticketId, User user, DateTime? createdAt = null) => new()
    {
        Id = Guid.NewGuid().ToString(),
        Body = "Test comment content",
        TicketId = ticketId,
        UserId = user.Id,
        User = user,
        CreatedAt = createdAt ?? DateTime.UtcNow
    };

    [Fact]
    public async Task AddAsync_AddsCommentToContext()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        var ticket = CreateTicket(user: user);
        context.SupportTickets.Add(ticket);
        await context.SaveChangesAsync();

        var repository = new CommentRepository(context);
        var comment = CreateComment(ticket.Id, user);

        await repository.AddAsync(comment);
        await context.SaveChangesAsync();

        var stored = await context.Comments.FindAsync(comment.Id);
        stored.Should().NotBeNull();
        stored!.Body.Should().Be("Test comment content");
    }

    [Fact]
    public async Task GetByTicketIdAsync_ReturnsCommentsForTicket()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        var ticket = CreateTicket(user: user);
        context.SupportTickets.Add(ticket);
        context.Comments.Add(CreateComment(ticket.Id, user));
        context.Comments.Add(CreateComment(ticket.Id, user));
        await context.SaveChangesAsync();

        var repository = new CommentRepository(context);

        var result = await repository.GetByTicketIdAsync(ticket.Id);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByTicketIdAsync_DoesNotReturnCommentsFromOtherTickets()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        var ticket1 = CreateTicket(user: user);
        var ticket2 = CreateTicket(user: user);
        context.SupportTickets.Add(ticket1);
        context.SupportTickets.Add(ticket2);
        context.Comments.Add(CreateComment(ticket1.Id, user));
        context.Comments.Add(CreateComment(ticket2.Id, user));
        await context.SaveChangesAsync();

        var repository = new CommentRepository(context);

        var result = await repository.GetByTicketIdAsync(ticket1.Id);

        result.Should().HaveCount(1);
        result[0].TicketId.Should().Be(ticket1.Id);
    }

    [Fact]
    public async Task GetByTicketIdAsync_ReturnsCommentsOrderedByCreatedAtDescending()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        var ticket = CreateTicket(user: user);
        context.SupportTickets.Add(ticket);
        
        var oldComment = CreateComment(ticket.Id, user, DateTime.UtcNow.AddDays(-2));
        var newComment = CreateComment(ticket.Id, user, DateTime.UtcNow);
        var middleComment = CreateComment(ticket.Id, user, DateTime.UtcNow.AddDays(-1));
        
        context.Comments.Add(oldComment);
        context.Comments.Add(newComment);
        context.Comments.Add(middleComment);
        await context.SaveChangesAsync();

        var repository = new CommentRepository(context);

        var result = await repository.GetByTicketIdAsync(ticket.Id);

        result.Should().HaveCount(3);
        result[0].CreatedAt.Should().BeOnOrAfter(result[1].CreatedAt);
        result[1].CreatedAt.Should().BeOnOrAfter(result[2].CreatedAt);
    }

    [Fact]
    public async Task GetByTicketIdAsync_IncludesUserNavigation()
    {
        using var context = CreateInMemoryContext();
        var user = CreateUser();
        context.Users.Add(user);
        var ticket = CreateTicket(user: user);
        context.SupportTickets.Add(ticket);
        context.Comments.Add(CreateComment(ticket.Id, user));
        await context.SaveChangesAsync();

        var repository = new CommentRepository(context);

        var result = await repository.GetByTicketIdAsync(ticket.Id);

        result[0].User.Should().NotBeNull();
        result[0].User!.UserName.Should().Be("testuser");
    }

    [Fact]
    public async Task GetByTicketIdAsync_WhenNoComments_ReturnsEmptyList()
    {
        using var context = CreateInMemoryContext();
        var repository = new CommentRepository(context);

        var result = await repository.GetByTicketIdAsync("non-existent-ticket");

        result.Should().BeEmpty();
    }
}
