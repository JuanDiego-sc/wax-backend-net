using Application.Core;
using Application.Interfaces;
using Application.Interfaces.Repositories;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Domain.Entities;
using Domain.OrderAggregate;
using MediatR;

namespace Application.Orders.Commands;

public class CreateOrderCommandHandler(
    IBasketRepository basketRepository,
    IOrderRepository orderRepository,
    IUnitOfWork unitOfWork,
    IUserAccessor userAccessor)
    : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var basket = await basketRepository.GetBasketWithItemsAsync(request.BasketId, cancellationToken);
        var user = await userAccessor.GetUserAsync();

        if (basket == null ||
            basket.Items.Count == 0 ||
            string.IsNullOrEmpty(basket.PaymentIntentId))
            return Result<OrderDto>.Failure("Basket not found or is empty");

        var items = CreateOrderItems(basket.Items);

        if (items == null) return Result<OrderDto>.Failure("One or more items in the basket are out of stock");

        var subtotal = items.Sum(x => x.Price * x.Quantity);
        var deliveryFee = CalculateDeliveryFee(subtotal);

        var order = await orderRepository.GetByPaymentIntentIdAsync(basket.PaymentIntentId, cancellationToken);

        if (order == null)
        {
            order = new Order
            {
                BuyerEmail = user.Email ?? string.Empty,
                ShippingAddress = request.OrderDto.BillingAddress,
                OrderItems = items,
                Subtotal = subtotal,
                DeliveryFee = deliveryFee,
                PaymentIntentId = basket.PaymentIntentId,
                PaymentSummary = request.OrderDto.PaymentSummary
            };

            orderRepository.Add(order);
        }
        else
        {
            order.OrderItems = items;
            orderRepository.Update(order);
        }

        var result = await unitOfWork.CompleteAsync(cancellationToken);

        return !result
            ? Result<OrderDto>.Failure("Failed to create order")
            : Result<OrderDto>.Success(order.ToDto());
    }

    #region Private Methods

    private static List<OrderItem>? CreateOrderItems(List<BasketItem> items)
    {
        var orderItems = new List<OrderItem>();

        foreach (var item in items)
        {
            if (item.Product.QuantityInStock < item.Quantity) return null;

            var orderItem = new OrderItem
            {
                ItemOrdered = new ProductOrderItem
                {
                    ProductId = item.Product.Id,
                    Name = item.Product.Name,
                },
                Price = item.Product.Price,
                Quantity = item.Quantity
            };

            orderItems.Add(orderItem);

            item.Product.QuantityInStock -= item.Quantity;
        }

        return orderItems;
    }

    private static long CalculateDeliveryFee(long subtotal)
    {
        return subtotal > 10000 ? 0 : 500;
    }

    #endregion
}
