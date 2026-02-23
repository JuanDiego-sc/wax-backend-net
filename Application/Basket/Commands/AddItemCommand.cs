using Application.Basket.DTOs;
using Application.Core;
using MediatR;

namespace Application.Basket.Commands;

public class AddItemCommand : IRequest<Result<BasketDto>>
{
    public required string ProductId { get; set; }
    public required int Quantity { get; set; }
    public required string BasketId { get; set; }
}
