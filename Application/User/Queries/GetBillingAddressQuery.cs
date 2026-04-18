using Application.Core;
using Application.Orders.DTOs;
using MediatR;

namespace Application.User.Queries;

public record GetBillingAddressQuery : IRequest<Result<BillingAddressDto>>;
