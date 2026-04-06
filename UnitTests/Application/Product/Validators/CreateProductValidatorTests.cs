using Application.Interfaces.DTOs;
using Application.Product.Commands;
using Application.Product.DTOs;
using Application.Product.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Product.Validators;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    private static CreateProductCommand CreateValidCommand() => new()
    {
        ProductDto = new CreateProductDto
        {
            Name = "Valid Product",
            Description = "Valid Description",
            Price = 1000,
            Type = "Resin",
            Brand = "WaxBrand",
            QuantityInStock = 10
        },
        ImageRequest = new ImageUploadRequest(Stream.Null, "test.jpg", "image/jpeg")
    };

    [Fact]
    public void Validate_WhenNameEmpty_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Name = "";

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.Name);
    }

    [Fact]
    public void Validate_WhenNameExceeds100Characters_HasError()
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
    public void Validate_WhenDescriptionExceeds500Characters_HasError()
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
    public void Validate_WhenPriceNegative_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.Price = -100;

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
    public void Validate_WhenQuantityInStockNegative_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.QuantityInStock = -1;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.QuantityInStock);
    }

    [Fact]
    public void Validate_WhenQuantityInStockExceeds200_HasError()
    {
        var command = CreateValidCommand();
        command.ProductDto.QuantityInStock = 201;

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.ProductDto.QuantityInStock);
    }

    [Fact]
    public void Validate_WhenValidCommand_NoErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
