using Application.SupportAssist.DTOs;
using Domain.SupportAssistAggregate;

namespace Application.SupportAssist.Extensions;

public static class SupportTicketExtensions
{
    public static SupportTicketDto ToDto(this SupportTicket ticket)
    {
        return new SupportTicketDto
        {
            Id = ticket.Id,
            UserId = ticket.UserId,
            OrderId = ticket.OrderId,
            Category = ticket.Category.ToString(),
            Status = ticket.Status.ToString(),
            Description = ticket.Description,
            Subject = ticket.Subject,
            CreatedAt = ticket.CreatedAt,
            UserEmail = ticket.User?.UserName ?? string.Empty,
            UserFullName = ticket.User?.UserName ?? string.Empty
        };
    }

    public static IQueryable<SupportTicket> Sort(this IQueryable<SupportTicket> query, string? orderBy)
    {
        query = orderBy switch
        {
            "dateAsc" => query.OrderBy(x => x.CreatedAt),
            "dateDesc" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
        return query;
    }

    public static IQueryable<SupportTicket> Filter(this IQueryable<SupportTicket> query, string? status, string? category, DateTime? createdOn)
    {
        var statusList = new List<string>();
        var categoryList = new List<string>();

        if (!string.IsNullOrEmpty(status))
        {
            statusList.AddRange([.. status.ToLower().Split(",")]);
        }

        if (!string.IsNullOrEmpty(category))
        {
            categoryList.AddRange([.. category.ToLower().Split(",")]);
        }

        query = query.Where(x => statusList.Count == 0 || statusList.Contains(x.Status.ToString().ToLower()));
        query = query.Where(x => categoryList.Count == 0 || categoryList.Contains(x.Category.ToString().ToLower()));

        if (createdOn.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Date == createdOn.Value.Date);
        }

        return query;
    }
}
