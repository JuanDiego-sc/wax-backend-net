using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using MediatR;

namespace Application.CustomProducts.Commands;

public class CustomerProposePriceCommand : IRequest<Result<CustomProductDto>>
{
    public required string CustomProductId { get; init; }
    public required string OwnerUserId { get; init; }
    public string? BasketId { get; init; }
    public required ProposeCustomPriceDto ProposeCustomPrice { get; init; }
}