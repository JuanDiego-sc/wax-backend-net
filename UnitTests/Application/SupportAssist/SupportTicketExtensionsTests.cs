using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using Domain.SupportAssistAggregate;
using UnitTests.Helpers.Fixtures;

namespace UnitTests.Application.SupportAssist;

public class SupportTicketExtensionsTests
{
    private static List<SupportTicketDto> BuildDtos(params (string Status, string Category, DateTime CreatedAt)[] entries)
    {
        return entries.Select(e => new SupportTicketDto
        {
            Id = Guid.NewGuid().ToString(),
            Subject = "Test",
            Description = "Desc",
            Status = e.Status,
            Category = e.Category,
            CreatedAt = e.CreatedAt,
            UserId = Guid.NewGuid().ToString(),
            OrderId = Guid.NewGuid().ToString()
        }).ToList();
    }

    // ---- Sort tests ----

    [Fact]
    public void Sort_WhenDateAsc_OrdersByCreatedAtAscending()
    {
        var now = DateTime.UtcNow;
        var dtos = BuildDtos(
            ("Open", "OrderIssue", now.AddDays(-1)),
            ("Open", "OrderIssue", now.AddDays(-3)),
            ("Open", "OrderIssue", now)
        );

        var result = dtos.AsQueryable().Sort("dateAsc").ToList();

        result[0].CreatedAt.Should().BeBefore(result[1].CreatedAt);
        result[1].CreatedAt.Should().BeBefore(result[2].CreatedAt);
    }

    [Fact]
    public void Sort_WhenDateDesc_OrdersByCreatedAtDescending()
    {
        var now = DateTime.UtcNow;
        var dtos = BuildDtos(
            ("Open", "OrderIssue", now.AddDays(-3)),
            ("Open", "OrderIssue", now),
            ("Open", "OrderIssue", now.AddDays(-1))
        );

        var result = dtos.AsQueryable().Sort("dateDesc").ToList();

        result[0].CreatedAt.Should().BeAfter(result[1].CreatedAt);
        result[1].CreatedAt.Should().BeAfter(result[2].CreatedAt);
    }

    [Fact]
    public void Sort_WhenNullOrUnknown_DefaultsToDescending()
    {
        var now = DateTime.UtcNow;
        var dtos = BuildDtos(
            ("Open", "OrderIssue", now.AddDays(-3)),
            ("Open", "OrderIssue", now),
            ("Open", "OrderIssue", now.AddDays(-1))
        );

        var resultNull = dtos.AsQueryable().Sort(null).ToList();
        var resultUnknown = dtos.AsQueryable().Sort("unknown").ToList();

        resultNull[0].CreatedAt.Should().BeAfter(resultNull[1].CreatedAt);
        resultUnknown[0].CreatedAt.Should().BeAfter(resultUnknown[1].CreatedAt);
    }

    // ---- Filter tests ----

    [Fact]
    public void Filter_WhenNoFilters_ReturnsAll()
    {
        var dtos = BuildDtos(
            ("Open", "OrderIssue", DateTime.UtcNow),
            ("Closed", "PaymentIssue", DateTime.UtcNow)
        );

        var result = dtos.AsQueryable().Filter(null, null, null).ToList();

        result.Should().HaveCount(2);
    }

    [Fact]
    public void Filter_ByStatus_ReturnsMatchingItems()
    {
        var dtos = BuildDtos(
            ("Open", "OrderIssue", DateTime.UtcNow),
            ("Closed", "PaymentIssue", DateTime.UtcNow),
            ("InProgress", "Other", DateTime.UtcNow)
        );

        var result = dtos.AsQueryable().Filter("Open", null, null).ToList();

        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Open");
    }

    [Fact]
    public void Filter_ByMultipleCommaSeparatedStatuses_ReturnsAll_Matching()
    {
        var dtos = BuildDtos(
            ("Open", "OrderIssue", DateTime.UtcNow),
            ("Closed", "PaymentIssue", DateTime.UtcNow),
            ("InProgress", "Other", DateTime.UtcNow)
        );

        var result = dtos.AsQueryable().Filter("Open,Closed", null, null).ToList();

        result.Should().HaveCount(2);
        result.Select(x => x.Status).Should().Contain("Open");
        result.Select(x => x.Status).Should().Contain("Closed");
    }

    [Fact]
    public void Filter_ByCategory_ReturnsMatchingItems()
    {
        var dtos = BuildDtos(
            ("Open", "OrderIssue", DateTime.UtcNow),
            ("Open", "PaymentIssue", DateTime.UtcNow)
        );

        var result = dtos.AsQueryable().Filter(null, "OrderIssue", null).ToList();

        result.Should().HaveCount(1);
        result[0].Category.Should().Be("OrderIssue");
    }

    [Fact]
    public void Filter_ByCreatedOnDate_ReturnsMatchingItems()
    {
        var targetDate = new DateTime(2025, 3, 15, 10, 0, 0, DateTimeKind.Utc);
        var otherDate = new DateTime(2025, 3, 16, 10, 0, 0, DateTimeKind.Utc);

        var dtos = BuildDtos(
            ("Open", "OrderIssue", targetDate),
            ("Open", "OrderIssue", otherDate)
        );

        var result = dtos.AsQueryable().Filter(null, null, targetDate).ToList();

        result.Should().HaveCount(1);
        result[0].CreatedAt.Should().Be(targetDate);
    }

    [Fact]
    public void Filter_CombinedStatusAndCategory_FiltersCorrectly()
    {
        var dtos = BuildDtos(
            ("Open", "OrderIssue", DateTime.UtcNow),
            ("Open", "PaymentIssue", DateTime.UtcNow),
            ("Closed", "OrderIssue", DateTime.UtcNow)
        );

        var result = dtos.AsQueryable().Filter("Open", "OrderIssue", null).ToList();

        result.Should().HaveCount(1);
        result[0].Status.Should().Be("Open");
        result[0].Category.Should().Be("OrderIssue");
    }

    // ---- ToDto tests ----

    [Fact]
    public void ToDto_MapsAllPropertiesCorrectly()
    {
        var ticket = SupportTicketFixtures.CreateSupportTicket(
            category: TicketCategory.PaymentIssue,
            status: TicketStatus.InProgress,
            subject: "Help needed",
            description: "Long description");

        var dto = ticket.ToDto();

        dto.Id.Should().Be(ticket.Id);
        dto.Subject.Should().Be("Help needed");
        dto.Description.Should().Be("Long description");
        dto.Status.Should().Be(TicketStatus.InProgress.ToString());
        dto.Category.Should().Be(TicketCategory.PaymentIssue.ToString());
        dto.UserId.Should().Be(ticket.UserId);
        dto.OrderId.Should().Be(ticket.OrderId);
        dto.CreatedAt.Should().Be(ticket.CreatedAt);
    }
}
