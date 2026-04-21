using Application.Core;
using Application.Core.Validations;
using Application.Orders.DTOs;
using Application.User.DTOs;
using MediatR;

namespace Application.User.Commands;

public class CreateOrUpdateBillingAddressCommand : IRequest<Result<BillingAddressDto>>
{
    public required CreateOrUpdateBillingInfoRequest BillingInfo { get; set; }
}