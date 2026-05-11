using Application.Core.Validations;
using MediatR;

namespace Application.CustomProducts.Commands;

public class RejectCustomProductCommand : IRequest<Result<bool>>
{
    public required string CustomProductId { get; set; }
    public required string Reason { get; set; }
}