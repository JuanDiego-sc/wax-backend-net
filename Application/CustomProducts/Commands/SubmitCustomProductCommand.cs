using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using MediatR;

namespace Application.CustomProducts.Commands;

public class SubmitCustomProductCommand : IRequest<Result<CustomProductDto>>
{
    public required SubmitCustomProductRequest CustomProduct { get; init; }
    public required string OwnerUserId { get; init; }
    public string? BasketId { get; init; }
}