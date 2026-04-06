using Application.Basket.Commands;
using Application.Basket.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Basket.Validators;

public class RemoveItemValidatorTests
{
    private readonly RemoveItemValidator _validator = new();

    [Fact]
    public void Validate_WhenProductIdEmpty_HasError()
    {
        var command = new RemoveBasketItemCommand
        {
            ProductId = "",
            BasketId = "basket-1",
            Quantity = 1
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductId);
    }

    [Fact]
    public void Validate_WhenBasketIdEmpty_HasError()
    {
        var command = new RemoveBasketItemCommand
        {
            ProductId = "product-1",
            BasketId = "",
            Quantity = 1
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.BasketId);
    }

    [Fact]
    public void Validate_WhenQuantityZero_HasError()
    {
        var command = new RemoveBasketItemCommand
        {
            ProductId = "product-1",
            BasketId = "basket-1",
            Quantity = 0
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Validate_WhenQuantityExceeds100_HasError()
    {
        var command = new RemoveBasketItemCommand
        {
            ProductId = "product-1",
            BasketId = "basket-1",
            Quantity = 101
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Validate_WhenValid_NoErrors()
    {
        var command = new RemoveBasketItemCommand
        {
            ProductId = "product-1",
            BasketId = "basket-1",
            Quantity = 5
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
