using Application.Core;
using Application.Core.Pagination;
using Application.Core.Validations;
using Application.SupportAssist.DTOs;
using Application.SupportAssist.Extensions;
using MediatR;

namespace Application.SupportAssist.Queries;

public record GetMyTicketsQuery(SupportTicketParams Params)
    : IRequest<Result<PagedList<SupportTicketDto>>>;
