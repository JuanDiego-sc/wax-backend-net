using Application.Core.Validations;
using Application.CustomProducts.DTOs;
using Domain.ProductAggregate;
using MediatR;

namespace Application.CustomProducts.Commands;

public class ApproveCustomProductPriceCommand : IRequest<Result<CustomProductDto>>
{ 
    public required string CustomProductId { get; set; }
    public required ProposalSource Approver { get; set; }
    public string? OwnerUserId { get; set; }
    public string? BasketId { get; set; }
} 