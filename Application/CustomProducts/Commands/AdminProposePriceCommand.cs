using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using MediatR;

namespace Application.CustomProducts.Commands;

public class AdminProposePriceCommand : IRequest<Result<CustomProductDto>>
{
    public required string CustomProductId { get; init; }
    public required ProposeCustomPriceDto ProposeCustomPrice { get; init; }
}