using Application.SupportAssist.DTOs;

namespace Application.SupportAssist.Extensions;

public static class SupportTicketDtoExtensions
{
    public static IQueryable<SupportTicketDto> Sort(this IQueryable<SupportTicketDto> query, string? orderBy)
    {
        query = orderBy switch
        {
            "dateAsc" => query.OrderBy(x => x.CreatedAt),
            "dateDesc" => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };
        return query;
    }

    public static IQueryable<SupportTicketDto> Filter(this IQueryable<SupportTicketDto> query, string? status, string? category, DateTime? createdOn)
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

        query = query.Where(x => statusList.Count == 0 || statusList.Contains(x.Status.ToLower()));
        query = query.Where(x => categoryList.Count == 0 || categoryList.Contains(x.Category.ToLower()));

        if (createdOn.HasValue)
        {
            query = query.Where(x => x.CreatedAt.Date == createdOn.Value.Date);
        }

        return query;
    }
}
