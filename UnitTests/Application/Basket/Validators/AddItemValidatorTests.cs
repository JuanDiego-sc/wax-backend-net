using Application.Basket.Commands;
using Application.Basket.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Basket.Validators;

public class AddItemValidatorTests
{
    private readonly AddItemValidator _validator = new();

    [Fact]
    public void Validate_WhenProductIdEmpty_HasError()
    {
        var command = new AddItemCommand
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
        var command = new AddItemCommand
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
        var command = new AddItemCommand
        {
            ProductId = "product-1",
            BasketId = "basket-1",
            Quantity = 0
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Validate_WhenQuantityNegative_HasError()
    {
        var command = new AddItemCommand
        {
            ProductId = "product-1",
            BasketId = "basket-1",
            Quantity = -1
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Validate_WhenQuantityExceeds100_HasError()
    {
        var command = new AddItemCommand
        {
            ProductId = "product-1",
            BasketId = "basket-1",
            Quantity = 101
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Quantity);
    }

    [Fact]
    public void Validate_WhenValidCommand_NoErrors()
    {
        var command = new AddItemCommand
        {
            ProductId = "product-1",
            BasketId = "basket-1",
            Quantity = 5
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WhenQuantityIs100_NoError()
    {
        var command = new AddItemCommand
        {
            ProductId = "product-1",
            BasketId = "basket-1",
            Quantity = 100
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Quantity);
    }
}
