using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.Interfaces.Repositories.ReadRepositories;
using Application.Interfaces.Services;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using MediatR;

namespace Application.SupportAssist.Queries;

public class GetMyTicketsQueryHandler(
    ISupportTicketReadRepository repository,
    IUserAccessor userAccessor)
    : IRequestHandler<GetMyTicketsQuery, Result<PagedList<SupportTicketDto>>>
{
    public async Task<Result<PagedList<SupportTicketDto>>> Handle(
        GetMyTicketsQuery request, CancellationToken cancellationToken)
    {
        var userId = userAccessor.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Result<PagedList<SupportTicketDto>>.Failure("User identity could not be resolved.", 401);

        var query = repository.GetSupportTickets()
            .Where(x => x.UserId == userId)
            .Sort(request.Params.OrderBy)
            .Filter(request.Params.Status, request.Params.Category, request.Params.CreatedOn);

        var paged = await PagedList<SupportTicketDto>.ToPagedList(
            query, request.Params.PageNumber, request.Params.PageSize);

        return Result<PagedList<SupportTicketDto>>.Success(paged);
    }
}
