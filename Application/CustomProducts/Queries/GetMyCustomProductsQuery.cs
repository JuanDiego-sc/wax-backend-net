using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using MediatR;

namespace Application.CustomProducts.Queries;

public class GetMyCustomProductsQuery : IRequest<Result<List<CustomProductDto>>>
{
    public required string OwnerUserId { get; set; }
}