using Domain.Entities;

namespace UnitTests.Helpers.Fixtures;

public static class BasketFixtures
{
    public static Basket CreateBasket(
        string? basketId = null,
        string? paymentIntentId = null,
        string? clientSecret = null)
    {
        return new Basket
        {
            BasketId = basketId ?? Guid.NewGuid().ToString(),
            PaymentIntentId = paymentIntentId,
            ClientSecret = clientSecret,
            Items = []
        };
    }

    public static Basket CreateBasketWithItems(
        string? basketId = null,
        string? paymentIntentId = null,
        List<BasketItem>? items = null)
    {
        var basket = CreateBasket(basketId, paymentIntentId);
        basket.Items = items ?? [CreateBasketItem()];
        return basket;
    }

    public static BasketItem CreateBasketItem(
        string? productId = null,
        int quantity = 2,
        Product? product = null)
    {
        var resolvedProduct = product ?? ProductFixtures.CreateProduct(productId);
        return new BasketItem
        {
            ProductId = resolvedProduct.Id,
            Product = resolvedProduct,
            Quantity = quantity
        };
    }
}
