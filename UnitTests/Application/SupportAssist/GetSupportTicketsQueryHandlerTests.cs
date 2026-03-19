using Application.Interfaces.Repositories.ReadRepositories;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using Application.SupportAssist.Queries;
using MockQueryable;

namespace UnitTests.Application.SupportAssist;

public class GetSupportTicketsQueryHandlerTests
{
    private static List<SupportTicketDto> CreateTicketDtos(int count, string? status = null, string? category = null)
    {
        var tickets = new List<SupportTicketDto>();
        for (var i = 0; i < count; i++)
        {
            tickets.Add(new SupportTicketDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid().ToString(),
                UserEmail = $"user{i}@test.com",
                UserFullName = $"User {i}",
                OrderId = Guid.NewGuid().ToString(),
                Category = category ?? "Other",
                Status = status ?? "Open",
                Subject = $"Test Subject {i}",
                Description = $"Test Description {i}",
                CreatedAt = DateTime.UtcNow
            });
        }
        return tickets;
    }

    [Fact]
    public async Task Handle_ReturnsAllTicketsWithoutFilter()
    {
        var tickets = CreateTicketDtos(3);
        var mockQueryable = tickets.BuildMock();

        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetSupportTickets()).Returns(mockQueryable);

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new SupportTicketParams { PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(3);
    }

    [Fact]
    public async Task Handle_WithStatusFilter_ReturnsOnlyMatchingTickets()
    {
        var tickets = new List<SupportTicketDto>
        {
            new() { Id = "1", UserId = "u1", UserEmail = "a@test.com", UserFullName = "A", OrderId = "o1", Category = "Other", Status = "Open", Subject = "S1", Description = "D1", CreatedAt = DateTime.UtcNow },
            new() { Id = "2", UserId = "u2", UserEmail = "b@test.com", UserFullName = "B", OrderId = "o2", Category = "Other", Status = "Closed", Subject = "S2", Description = "D2", CreatedAt = DateTime.UtcNow },
            new() { Id = "3", UserId = "u3", UserEmail = "c@test.com", UserFullName = "C", OrderId = "o3", Category = "Other", Status = "Open", Subject = "S3", Description = "D3", CreatedAt = DateTime.UtcNow }
        };
        var mockQueryable = tickets.BuildMock();

        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetSupportTickets()).Returns(mockQueryable);

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new SupportTicketParams { Status = "open", PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_ReturnsOnlyMatchingTickets()
    {
        var tickets = new List<SupportTicketDto>
        {
            new() { Id = "1", UserId = "u1", UserEmail = "a@test.com", UserFullName = "A", OrderId = "o1", Category = "OrderIssue", Status = "Open", Subject = "S1", Description = "D1", CreatedAt = DateTime.UtcNow },
            new() { Id = "2", UserId = "u2", UserEmail = "b@test.com", UserFullName = "B", OrderId = "o2", Category = "OrderIssue", Status = "Open", Subject = "S2", Description = "D2", CreatedAt = DateTime.UtcNow },
            new() { Id = "3", UserId = "u3", UserEmail = "c@test.com", UserFullName = "C", OrderId = "o3", Category = "ProductIssue", Status = "Open", Subject = "S3", Description = "D3", CreatedAt = DateTime.UtcNow }
        };
        var mockQueryable = tickets.BuildMock();

        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetSupportTickets()).Returns(mockQueryable);

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new SupportTicketParams { Category = "orderissue", PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithCreatedOnFilter_ReturnsOnlyMatchingTickets()
    {
        var today = DateTime.UtcNow;
        var yesterday = today.AddDays(-1);

        var tickets = new List<SupportTicketDto>
        {
            new() { Id = "1", UserId = "u1", UserEmail = "a@test.com", UserFullName = "A", OrderId = "o1", Category = "Other", Status = "Open", Subject = "S1", Description = "D1", CreatedAt = yesterday },
            new() { Id = "2", UserId = "u2", UserEmail = "b@test.com", UserFullName = "B", OrderId = "o2", Category = "Other", Status = "Open", Subject = "S2", Description = "D2", CreatedAt = today },
            new() { Id = "3", UserId = "u3", UserEmail = "c@test.com", UserFullName = "C", OrderId = "o3", Category = "Other", Status = "Open", Subject = "S3", Description = "D3", CreatedAt = today }
        };
        var mockQueryable = tickets.BuildMock();

        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetSupportTickets()).Returns(mockQueryable);

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new SupportTicketParams { CreatedOn = today.Date, PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithMultipleFilters_ReturnsMatchingTickets()
    {
        var tickets = new List<SupportTicketDto>
        {
            new() { Id = "1", UserId = "u1", UserEmail = "a@test.com", UserFullName = "A", OrderId = "o1", Category = "OrderIssue", Status = "Open", Subject = "S1", Description = "D1", CreatedAt = DateTime.UtcNow },
            new() { Id = "2", UserId = "u2", UserEmail = "b@test.com", UserFullName = "B", OrderId = "o2", Category = "PaymentIssue", Status = "Open", Subject = "S2", Description = "D2", CreatedAt = DateTime.UtcNow },
            new() { Id = "3", UserId = "u3", UserEmail = "c@test.com", UserFullName = "C", OrderId = "o3", Category = "ProductIssue", Status = "Closed", Subject = "S3", Description = "D3", CreatedAt = DateTime.UtcNow }
        };
        var mockQueryable = tickets.BuildMock();

        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetSupportTickets()).Returns(mockQueryable);

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new SupportTicketParams { Status = "open", Category = "orderissue", PageSize = 10, PageNumber = 1 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        var tickets = CreateTicketDtos(15);
        var mockQueryable = tickets.BuildMock();

        var repoMock = new Mock<ISupportTicketReadRepository>();
        repoMock.Setup(r => r.GetSupportTickets()).Returns(mockQueryable);

        var handler = new GetSupportTicketsQueryHandler(repoMock.Object);
        var query = new GetSupportTicketsQuery
        {
            TicketParams = new SupportTicketParams { PageSize = 5, PageNumber = 2 }
        };

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(5);
        result.Value.Metadata.CurrentPage.Should().Be(2);
    }
}
