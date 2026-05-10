using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using MediatR;

namespace Application.CustomProducts.Queries;

public class GetCustomProductDetailsQuery : IRequest<Result<CustomProductDto>>
{
    public required string Id { get; set; }
    public required string RequesterUserId { get; set; }
    public bool RequesterIsAdmin { get; set; }
}