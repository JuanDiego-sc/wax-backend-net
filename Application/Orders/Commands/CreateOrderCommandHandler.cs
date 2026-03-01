using System;
using Application.Basket.Extensions;
using Application.Core;
using Application.Interfaces;
using Application.Orders.DTOs;
using Application.Orders.Extensions;
using Domain.Entities;
using Domain.OrderAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Orders.Commands;

public class CreateOrderCommandHandler(AppDbContext context, IUserAccessor userAccessor) : IRequestHandler<CreateOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var basket = await context.Baskets.GetBasketWithItems(request.BasketId);
        var user = await userAccessor.GetUserAsync();

        if (basket == null ||
         basket.Items.Count == 0 ||
          string.IsNullOrEmpty(basket.PaymentIntentId)) return Result<OrderDto>.Failure("Basket not found or is empty");

        var items = CreateOrderItems(basket.Items);

        if (items == null) return Result<OrderDto>.Failure("One or more items in the basket are out of stock");

        var subtotal = items.Sum(x => x.Price * x.Quantity);
        var deliveryFee = CalculateDeliveryFee(subtotal);

        var order = await context.Orders
            .Include(x => x.OrderItems)
            .FirstOrDefaultAsync(x => x.PaymentIntentId == basket.PaymentIntentId, cancellationToken);
        
        if(order == null)
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

            context.Orders.Add(order);
        }
        else
        {
            order.OrderItems = items;
        }

        var result = await context.SaveChangesAsync(cancellationToken) > 0;

        if (!result) return Result<OrderDto>.Failure("Failed to create order");
        
        return Result<OrderDto>.Success(order.ToDto());
    }





    #region private methods

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
