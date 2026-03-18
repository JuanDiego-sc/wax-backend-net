using Application.Interfaces.Repositories;
using Application.Interfaces.Repositories.WriteRepositores;
using Application.SupportAssist.Extensions;
using Application.SupportAssist.Queries;
using Domain.SupportAssistAggregate;
using Microsoft.EntityFrameworkCore;
using Moq;
using Persistence;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.SupportAssist;

public class GetSupportTicketsQueryHandlerTests
{
    private static WriteDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<WriteDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new WriteDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsAllTicketsWithoutFilter()
    {
        using var context = CreateInMemoryContext();
        context.SupportTickets.AddRange(
            SupportTicketFixtures.CreateSupportTicket(),
            SupportTicketFixtures.CreateSupportTicket(),
            SupportTicketFixtures.CreateSupportTicket());
        await context.SaveChangesAsync();

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.SupportTickets.Include(t => t.User).AsQueryable());

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new() { PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyMatchingTickets()
    {
        using var context = CreateInMemoryContext();
        context.SupportTickets.AddRange(
            SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Open),
            SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Closed),
            SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Open));
        await context.SaveChangesAsync();

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.SupportTickets.Include(t => t.User).AsQueryable());

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new() { Status = "open", PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ReturnsOnlyMatchingTickets()
    {
        using var context = CreateInMemoryContext();
        context.SupportTickets.AddRange(
            SupportTicketFixtures.CreateSupportTicket(category: TicketCategory.OrderIssue),
            SupportTicketFixtures.CreateSupportTicket(category: TicketCategory.OrderIssue),
            SupportTicketFixtures.CreateSupportTicket(category: TicketCategory.ProductIssue));
        await context.SaveChangesAsync();

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.SupportTickets.Include(t => t.User).AsQueryable());

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new() { Category = "orderIssue", PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithCreatedOnFilter_ReturnsOnlyMatchingTickets()
    {
        using var context = CreateInMemoryContext();
        var today = DateTime.UtcNow;
        var yesterday = today.AddDays(-1);

        var ticket1 = SupportTicketFixtures.CreateSupportTicket();
        var ticket2 = SupportTicketFixtures.CreateSupportTicket();
        var ticket3 = SupportTicketFixtures.CreateSupportTicket();

        context.SupportTickets.AddRange(ticket1, ticket2, ticket3);
        await context.SaveChangesAsync();

        ticket1.CreatedAt = yesterday;
        ticket2.CreatedAt = today;
        ticket3.CreatedAt = today;
        await context.SaveChangesAsync();

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.SupportTickets.Include(t => t.User).AsQueryable());

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new() { CreatedOn = today.Date, PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithMultipleFilters_ReturnsMatchingTickets()
    {
        using var context = CreateInMemoryContext();
        context.SupportTickets.AddRange(
            SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Open, category: TicketCategory.OrderIssue),
            SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Open, category: TicketCategory.PaymentIssue),
            SupportTicketFixtures.CreateSupportTicket(status: TicketStatus.Closed, category: TicketCategory.ProductIssue));
        await context.SaveChangesAsync();

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.SupportTickets.Include(t => t.User).AsQueryable());

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new() { Status = "open", Category = "orderIssue", PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        using var context = CreateInMemoryContext();
        for (var i = 0; i < 15; i++)
            context.SupportTickets.Add(SupportTicketFixtures.CreateSupportTicket());
        await context.SaveChangesAsync();

        var repoMock = new Mock<ISupportTicketRepository>();
        repoMock.Setup(r => r.GetQueryable()).Returns(context.SupportTickets.Include(t => t.User).AsQueryable());

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new() { PageSize = 5, PageNumber = 2 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(5);
        result.Value.Metadata.CurrentPage.Should().Be(2);
    }
}
