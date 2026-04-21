using Application.Orders.DTOs;
using Domain.Entities;

namespace Application.User.Extensions;

public static class AddressExtension
{
    public static BillingAddressDto ToDto(this BillingAddress billingAddress)
    {
        return new BillingAddressDto
        {
            Name = billingAddress.Name,
            Line1 = billingAddress.Line1,
            Line2 = billingAddress.Line2,
            City = billingAddress.City,
            State = billingAddress.State,
            PostalCode = billingAddress.PostalCode,
            Country = billingAddress.Country
        };
    }
}