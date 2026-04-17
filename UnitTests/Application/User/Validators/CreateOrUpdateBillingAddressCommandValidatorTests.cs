using Application.User.Commands;
using Application.User.DTOs;
using Application.User.Validators;
using FluentValidation.TestHelper;

namespace UnitTests.Application.Users.Validators;

public class CreateOrUpdateBillingAddressCommandValidatorTests
{
    private readonly CreateOrUpdateBillingAddressCommandValidator _validator = new();

    private static CreateOrUpdateBillingAddressCommand CreateValidCommand() => new()
    {
        BillingInfo = new CreateOrUpdateBillingInfoRequest
        {
            Name = "Jane Doe",
            Line1 = "Street 123",
            Line2 = "Apt 2",
            City = "Quito",
            State = "Pichincha",
            PostalCode = "170101",
            Country = "EC",
            IdentificationNumber = "1723456789",
            IdentificationType = "CI",
            FirstName = "Jane",
            LastName = "Doe",
            Phone = "+593999999999"
        }
    };

    [Fact]
    public void Validate_WhenNameIsEmpty_HasValidationError()
    {
        var command = CreateValidCommand();
        command.BillingInfo.Name = string.Empty;

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BillingInfo.Name);
    }

    [Fact]
    public void Validate_WhenIdentificationTypeExceeds20_HasValidationError()
    {
        var command = CreateValidCommand();
        command.BillingInfo.IdentificationType = new string('A', 21);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BillingInfo.IdentificationType);
    }

    [Fact]
    public void Validate_WhenPhoneIsEmpty_HasValidationError()
    {
        var command = CreateValidCommand();
        command.BillingInfo.Phone = string.Empty;

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.BillingInfo.Phone);
    }

    [Fact]
    public void Validate_WhenValidCommand_HasNoValidationErrors()
    {
        var command = CreateValidCommand();

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
