using Application.Payment.Commands;
using Application.Payment.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Payment.Validators;

public class HandleStripeWebhookValidatorTests
{
    private readonly HandleStripeWebhookValidator _validator = new();

    [Fact]
    public void Validate_WhenPayloadEmpty_HasError()
    {
        var command = new HandleStripeWebhookCommand
        {
            Payload = "",
            Signature = "valid_signature"
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Payload);
    }

    [Fact]
    public void Validate_WhenSignatureEmpty_HasError()
    {
        var command = new HandleStripeWebhookCommand
        {
            Payload = "valid_payload",
            Signature = ""
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Signature);
    }

    [Fact]
    public void Validate_WhenBothEmpty_HasErrors()
    {
        var command = new HandleStripeWebhookCommand
        {
            Payload = "",
            Signature = ""
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Payload);
        result.ShouldHaveValidationErrorFor(x => x.Signature);
    }

    [Fact]
    public void Validate_WhenValid_NoErrors()
    {
        var command = new HandleStripeWebhookCommand
        {
            Payload = "valid_payload",
            Signature = "valid_signature"
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
