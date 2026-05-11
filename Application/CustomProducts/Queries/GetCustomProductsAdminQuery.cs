using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using MediatR;

namespace Application.CustomProducts.Queries;

public class GetCustomProductsAdminQuery : IRequest<Result<List<CustomProductDto>>>
{
    public string? Status { get; set; }
}