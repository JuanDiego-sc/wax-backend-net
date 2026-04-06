using Application.Payment.Commands;
using Application.Payment.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Payment.Validators;

public class CreateOrUpdateIntentValidatorTests
{
    private readonly CreateOrUpdateIntentValidator _validator = new();

    [Fact]
    public void Validate_WhenBasketIdEmpty_HasError()
    {
        var command = new CreateOrUpdateIntentCommand { BasketId = "" };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.BasketId);
    }

    [Fact]
    public void Validate_WhenBasketIdNull_HasError()
    {
        var command = new CreateOrUpdateIntentCommand { BasketId = null! };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.BasketId);
    }

    [Fact]
    public void Validate_WhenBasketIdValid_NoErrors()
    {
        var command = new CreateOrUpdateIntentCommand { BasketId = "basket-123" };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
