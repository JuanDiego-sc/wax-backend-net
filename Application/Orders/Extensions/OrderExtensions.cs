using System;
using Application.Orders.DTOs;
using Domain.OrderAggregate;

namespace Application.Orders.Extensions;

public static class OrderExtensions
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            BuyerEmail = order.BuyerEmail,
            CreateAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            BillingAddress = new BillingAddressDto
            {
                Name = order.BillingAddress.Name,
                Line1 = order.BillingAddress.Line1,
                Line2 = order.BillingAddress.Line2,
                City = order.BillingAddress.City,
                State = order.BillingAddress.State,
                PostalCode = order.BillingAddress.PostalCode,
                Country = order.BillingAddress.Country
            },
            PaymentSummary = new PaymentSummaryDto
            {
                Last4 = order.PaymentSummary.Last4,
                Brand = order.PaymentSummary.Brand,
                ExpMonth = order.PaymentSummary.ExpMonth,
                ExpYear = order.PaymentSummary.ExpYear
            },
            DeliveryFee = order.DeliveryFee,
            Subtotal = order.Subtotal,
            OrderStatus = order.OrderStatus.ToString(),
            Total = order.GetTotal(),
            OrderItems = [.. order.OrderItems.Select(item => new OrderItemDto
            {
                ProductId = item.ItemOrdered.ProductId,
                Name = item.ItemOrdered.Name,
                Price = item.Price,
                Quantity = item.Quantity
            })]
        };
    }
}
