using Application.Product.Commands;
using Application.Product.DTOs;
using Application.Product.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Product.Validators;

public class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _validator = new();

    private static UpdateProductCommand CreateValidCommand() => new()
    {
        ProductDto = new UpdateProductDto
        {
            Id = "product-1",
            Name = "Valid Product",
            Description = "Valid Description",
            Price = 1000,
            Type = "Resin",
            Brand = "WaxBrand",
            QuantityInStock = 10
        }
    };

    [Fact]
    public void Validate_WhenIdEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Id = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.Id);
    }

    [Fact]
    public void Validate_WhenNameEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Name = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.Name);
    }

    [Fact]
    public void Validate_WhenNameExceeds100_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Name = new string('a', 101);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.Name);
    }

    [Fact]
    public void Validate_WhenDescriptionEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Description = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.Description);
    }

    [Fact]
    public void Validate_WhenDescriptionExceeds500_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Description = new string('a', 501);

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.Description);
    }

    [Fact]
    public void Validate_WhenPriceZero_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Price = 0;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.Price);
    }

    [Fact]
    public void Validate_WhenTypeEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Type = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.Type);
    }

    [Fact]
    public void Validate_WhenBrandEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Brand = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.Brand);
    }

    [Fact]
    public void Validate_WhenQuantityNegative_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.QuantityInStock = -1;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.QuantityInStock);
    }

    [Fact]
    public void Validate_WhenQuantityExceeds200_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.QuantityInStock = 201;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.QuantityInStock);
    }

    [Fact]
    public void Validate_WhenValid_NoErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
