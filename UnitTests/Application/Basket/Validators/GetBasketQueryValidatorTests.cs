using Application.Basket.Queries;
using Application.Basket.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Basket.Validators;

public class GetBasketQueryValidatorTests
{
    private readonly GetBasketQueryValidator _validator = new();

    [Fact]
    public void Validate_WhenBasketIdEmpty_HasError()
    {
        var query = new GetBasketQuery { BasketId = "" };

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BasketId);
    }

    [Fact]
    public void Validate_WhenBasketIdNull_HasError()
    {
        var query = new GetBasketQuery { BasketId = null! };

        var result = _validator.TestValidate(query);
        result.ShouldHaveValidationErrorFor(x => x.BasketId);
    }

    [Fact]
    public void Validate_WhenBasketIdValid_NoErrors()
    {
        var query = new GetBasketQuery { BasketId = "basket-123" };

        var result = _validator.TestValidate(query);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
