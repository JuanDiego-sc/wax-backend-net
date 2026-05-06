using Application.Core.Validations;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Interfaces.Services;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using Application.SupportAssist.Queries;
using MockQueryable;

namespace UnitTests.Application.SupportAssist;

public class GetMyTicketsQueryHandlerTests
{
    private const string UserId = "user-abc-123";

    private static List<SupportTicketDto> CreateTicketDtos(int count, string? userId = null, string? status = null)
    {
        var tickets = new List<SupportTicketDto>();
        for (var i = 0; i < count; i++)
        {
            tickets.Add(new SupportTicketDto
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId ?? UserId,
                UserEmail = $"user{i}@test.com",
                UserFullName = $"User {i}",
                OrderId = Guid.NewGuid().ToString(),
                Category = "Other",
                Status = status ?? "Open",
                Subject = $"Test Subject {i}",
                Description = $"Test Description {i}",
                CreatedAt = DateTime.UtcNow
            });
        }
        return tickets;
    }

    private static (Mock<ISupportTicketReadRepository> Repo, Mock<IUserAccessor> UserAccessor, GetMyTicketsQueryHandler Handler) Build(
        List<SupportTicketDto> allTickets)
    {
        var repo = new Mock<ISupportTicketReadRepository>();
        repo.Setup(r => r.GetSupportTickets()).Returns(allTickets.BuildMock());

        var userAccessor = new Mock<IUserAccessor>();
        userAccessor.Setup(u => u.GetUserId()).Returns(UserId);

        var handler = new GetMyTicketsQueryHandler(repo.Object, userAccessor.Object);
        return (repo, userAccessor, handler);
    }

    [Fact]
    public async Task Handle_ReturnsOnlyTicketsBelongingToCurrentUser()
    {
        var myTickets = CreateTicketDtos(3, userId: UserId);
        var otherTickets = CreateTicketDtos(2, userId: "other-user");
        var allTickets = myTickets.Concat(otherTickets).ToList();

        var (_, _, handler) = Build(allTickets);
        var query = new GetMyTicketsQuery(new SupportTicketParams { PageSize = 10, PageNumber = 1 });

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(3);
        result.Value.Should().OnlyContain(t => t.UserId == UserId);
    }

    [Fact]
    public async Task Handle_WithNoMatchingTickets_ReturnsEmptyPagedList()
    {
        var otherTickets = CreateTicketDtos(3, userId: "different-user");

        var (_, _, handler) = Build(otherTickets);
        var query = new GetMyTicketsQuery(new SupportTicketParams { PageSize = 10, PageNumber = 1 });

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_AppliesStatusFilterWithinUserScope()
    {
        var tickets = new List<SupportTicketDto>
        {
            new() { Id = "1", UserId = UserId, UserEmail = "a@test.com", UserFullName = "A", OrderId = "o1", Category = "Other", Status = "Open", Subject = "S1", Description = "D1", CreatedAt = DateTime.UtcNow },
            new() { Id = "2", UserId = UserId, UserEmail = "a@test.com", UserFullName = "A", OrderId = "o2", Category = "Other", Status = "Closed", Subject = "S2", Description = "D2", CreatedAt = DateTime.UtcNow },
            new() { Id = "3", UserId = "other-user", UserEmail = "b@test.com", UserFullName = "B", OrderId = "o3", Category = "Other", Status = "Open", Subject = "S3", Description = "D3", CreatedAt = DateTime.UtcNow }
        };

        var (_, _, handler) = Build(tickets);
        var query = new GetMyTicketsQuery(new SupportTicketParams { Status = "open", PageSize = 10, PageNumber = 1 });

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(1);
        result.Value[0].Id.Should().Be("1");
    }

    [Fact]
    public async Task Handle_CallsGetUserIdOnce()
    {
        var (_, userAccessor, handler) = Build(CreateTicketDtos(2));
        var query = new GetMyTicketsQuery(new SupportTicketParams { PageSize = 10, PageNumber = 1 });

        await handler.Handle(query, CancellationToken.None);

        userAccessor.Verify(u => u.GetUserId(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPagination_ReturnsCorrectPage()
    {
        var tickets = CreateTicketDtos(8, userId: UserId);

        var (_, _, handler) = Build(tickets);
        var query = new GetMyTicketsQuery(new SupportTicketParams { PageSize = 3, PageNumber = 2 });

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Count.Should().Be(3);
        result.Value.Metadata.CurrentPage.Should().Be(2);
        result.Value.Metadata.TotalCount.Should().Be(8);
    }

    [Fact]
    public async Task Handle_WhenUserIdIsNull_ReturnsFailure()
    {
        var repo = new Mock<ISupportTicketReadRepository>();
        var userAccessor = new Mock<IUserAccessor>();
        userAccessor.Setup(u => u.GetUserId()).Returns(default(string));
        var handler = new GetMyTicketsQueryHandler(repo.Object, userAccessor.Object);
        var query = new GetMyTicketsQuery(new SupportTicketParams { PageSize = 10, PageNumber = 1 });

        var result = await handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        repo.Verify(r => r.GetSupportTickets(), Times.Never);
    }
}
